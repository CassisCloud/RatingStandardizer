using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

internal sealed class JsonShardRatingHistoryStore : IRatingHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly object _syncRoot = new();
    private readonly string _recordsPath;
    private readonly string _indexesPath;

    public JsonShardRatingHistoryStore(string rootPath)
    {
        Directory.CreateDirectory(rootPath);
        _recordsPath = Path.Combine(rootPath, "records");
        _indexesPath = Path.Combine(rootPath, "indexes");
        Directory.CreateDirectory(_recordsPath);
        Directory.CreateDirectory(_indexesPath);
        WriteStoreVersion(rootPath);
    }

    public RatingHistoryEntry? Get(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        lock (_syncRoot)
        {
            var shard = ReadShard(GetShardPath(itemId));
            return shard.TryGetValue(itemId, out var entry) ? entry : null;
        }
    }

    public IReadOnlyList<RatingHistoryEntry> GetAll()
    {
        lock (_syncRoot)
        {
            if (!Directory.Exists(_recordsPath))
            {
                return [];
            }

            return Directory
                .EnumerateFiles(_recordsPath, "*.json")
                .SelectMany(path => ReadShard(path).Values)
                .Where(static entry => !string.IsNullOrWhiteSpace(entry.ItemId))
                .GroupBy(static entry => entry.ItemId, StringComparer.Ordinal)
                .Select(static group => group.OrderByDescending(static entry => entry.UpdatedAt).First())
                .OrderByDescending(static entry => entry.UpdatedAt)
                .ThenBy(static entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    public void Save(RatingHistoryEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.ItemId))
        {
            return;
        }

        lock (_syncRoot)
        {
            var shardKey = GetShardKey(entry.ItemId);
            var shardPath = GetShardPathFromKey(shardKey);
            var shard = ReadShard(shardPath);

            if (shard.TryGetValue(entry.ItemId, out var existing))
            {
                if (!string.IsNullOrWhiteSpace(existing.OriginalOfficialRating))
                {
                    entry.OriginalOfficialRating = existing.OriginalOfficialRating;
                }

                if (!string.IsNullOrWhiteSpace(existing.OriginalCustomRating))
                {
                    entry.OriginalCustomRating = existing.OriginalCustomRating;
                }
            }

            shard[entry.ItemId] = entry;
            WriteJson(shardPath, shard);
            UpdateIndexes(entry, shardKey);
        }
    }

    public void SaveMany(IEnumerable<RatingHistoryEntry> entries)
    {
        foreach (var entry in entries ?? [])
        {
            Save(entry);
        }
    }

    private void UpdateIndexes(RatingHistoryEntry entry, string shardKey)
    {
        var itemsPath = Path.Combine(_indexesPath, "items.json");
        var itemShards = ReadDictionary<string>(itemsPath);
        itemShards[entry.ItemId] = shardKey;
        WriteJson(itemsPath, itemShards);

        var finalRatingPath = Path.Combine(_indexesPath, "by-final-rating.json");
        var byFinalRating = ReadDictionary<List<string>>(finalRatingPath);
        foreach (var key in byFinalRating.Keys.ToList())
        {
            byFinalRating[key].Remove(entry.ItemId);
            if (byFinalRating[key].Count == 0)
            {
                byFinalRating.Remove(key);
            }
        }

        if (!string.IsNullOrWhiteSpace(entry.FinalRating))
        {
            if (!byFinalRating.TryGetValue(entry.FinalRating, out var ids))
            {
                ids = [];
                byFinalRating[entry.FinalRating] = ids;
            }

            if (!ids.Contains(entry.ItemId, StringComparer.Ordinal))
            {
                ids.Add(entry.ItemId);
                ids.Sort(StringComparer.Ordinal);
            }
        }

        WriteJson(finalRatingPath, byFinalRating);
    }

    private Dictionary<string, RatingHistoryEntry> ReadShard(string path)
    {
        return ReadDictionary<RatingHistoryEntry>(path);
    }

    private Dictionary<string, TValue> ReadDictionary<TValue>(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, TValue>(StringComparer.Ordinal);
        }

        try
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            return JsonSerializer.Deserialize<Dictionary<string, TValue>>(json, JsonOptions) ?? new Dictionary<string, TValue>(StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return new Dictionary<string, TValue>(StringComparer.Ordinal);
        }
        catch (IOException)
        {
            return new Dictionary<string, TValue>(StringComparer.Ordinal);
        }
    }

    private static void WriteJson<TValue>(string path, TValue value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        var tempPath = path + ".tmp";
        var json = JsonSerializer.Serialize(value, JsonOptions);
        File.WriteAllText(tempPath, json, Encoding.UTF8);
        File.Move(tempPath, path, true);
    }

    private void WriteStoreVersion(string rootPath)
    {
        var versionPath = Path.Combine(rootPath, "store-version.json");
        if (!File.Exists(versionPath))
        {
            WriteJson(versionPath, new StoreVersion { Version = 1, Format = "json-shards-256" });
        }
    }

    private string GetShardPath(string itemId)
    {
        return GetShardPathFromKey(GetShardKey(itemId));
    }

    private string GetShardPathFromKey(string shardKey)
    {
        return Path.Combine(_recordsPath, shardKey + ".json");
    }

    private static string GetShardKey(string itemId)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(itemId));
        return hash[0].ToString("x2");
    }

    private sealed class StoreVersion
    {
        public int Version { get; set; }

        public string Format { get; set; } = string.Empty;
    }
}
