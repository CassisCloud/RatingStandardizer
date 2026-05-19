using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using RatingStandardizer.Core;

namespace RatingStandardizer.Jellyfin;

/// <summary>
/// Runs batch rating standardization for existing library items.
/// </summary>
internal static class RatingStandardizerBatchRunner
{
    private static readonly SemaphoreSlim RunLock = new(1, 1);
    private static ILibraryManager? _libraryManager;
    private static ILogger? _logger;
    private static readonly RatingConverter RatingConverter = new();

    public static void Initialize(ILibraryManager libraryManager, ILogger logger)
    {
        _libraryManager ??= libraryManager;
        _logger ??= logger;
    }

    public static async Task<RatingStandardizerBatchRunResult> RunAsync(IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var libraryManager = _libraryManager ?? throw new InvalidOperationException("Batch runner is not initialized.");
        var logger = _logger ?? throw new InvalidOperationException("Batch runner logger is not initialized.");

        if (!await RunLock.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Rating standardization is already running.");
        }

        try
        {
            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                logger.LogWarning("Plugin configuration is unavailable; skipping batch rating standardization.");
                progress?.Report(100);
                return RatingStandardizerBatchRunResult.ConfigurationUnavailable;
            }

            if (!configuration.IsEnabled)
            {
                logger.LogInformation("Rating Standardizer is disabled. Batch run skipped.");
                progress?.Report(100);
                return RatingStandardizerBatchRunResult.Disabled;
            }

            var items = libraryManager
                .GetUserRootFolder()
                .GetRecursiveChildren()
                .Concat(libraryManager.RootFolder.GetRecursiveChildren())
                .Concat(libraryManager.GetItemList(new InternalItemsQuery { MediaTypes = [global::Jellyfin.Data.Enums.MediaType.Video], Recursive = true }))
                .Where(static item => item is Video or MediaBrowser.Controller.Entities.TV.Series)
                .GroupBy(static item => item.Id)
                .Select(static group => group.First())
                .ToList();

            var targetLookup = LibraryFilter.CreateTargetLookup(configuration.TargetLibraryIds);
            if (targetLookup is not null)
            {
                var filteredItems = items.Where(item => LibraryFilter.IsMatch(item, targetLookup)).ToList();
                if (filteredItems.Count > 0)
                {
                    items = filteredItems;
                }
                else
                {
                    logger.LogWarning(
                        "Target library filter matched no items. Running unfiltered so Run Now still processes available movies and series. TargetLibraryCount={TargetLibraryCount}",
                        configuration.TargetLibraryIds.Count);
                }
            }

            var totalCount = items.Count;
            var updatedCount = 0;
            var matchedCount = 0;
            var alreadyStandardizedCount = 0;
            var missingOfficialRatingCount = 0;
            var noMatchingRuleCount = 0;

            for (var index = 0; index < totalCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items[index];
                var libraryProfile = LibraryFilter.FindProfile(item, configuration.LibraryProfiles);
                var effectiveConfiguration = RatingConfigurationResolver.Resolve(configuration, libraryProfile);
                var result = ItemRatingStandardizer.Apply(item, effectiveConfiguration, RatingConverter, Plugin.HistoryStore, Plugin.PluginVersion);
                if (result.MatchedMapping)
                {
                    matchedCount++;

                    if (result.Status == ItemRatingStandardizationStatus.AlreadyStandardized)
                    {
                        alreadyStandardizedCount++;
                    }
                }
                else
                {
                    if (result.Status == ItemRatingStandardizationStatus.MissingOfficialRating)
                    {
                        missingOfficialRatingCount++;
                    }
                    else if (result.Status == ItemRatingStandardizationStatus.NoMatchingRule)
                    {
                        noMatchingRuleCount++;
                    }

                    logger.LogDebug(
                        "Skipped item {ItemName}. OfficialRating={OfficialRating}, Reason={Reason}.",
                        item.Name,
                        item.OfficialRating,
                        result.Status);
                }

                if (result.RequiresSave)
                {
                    var parent = item.GetParent() ?? item;
                    await libraryManager.UpdateItemAsync(item, parent, ItemUpdateType.MetadataEdit, cancellationToken).ConfigureAwait(false);
                    updatedCount++;
                }

                if (totalCount > 0)
                {
                    progress?.Report(((double)(index + 1) / totalCount) * 100);
                }
            }

            if (totalCount == 0)
            {
                progress?.Report(100);
            }

            logger.LogInformation(
                "Rating Standardizer batch run completed. Scanned {ScannedCount} items, matched {MatchedCount}, updated {UpdatedCount}, already standardized {AlreadyStandardizedCount}, missing official rating {MissingOfficialRatingCount}, no matching rule {NoMatchingRuleCount}.",
                totalCount,
                matchedCount,
                updatedCount,
                alreadyStandardizedCount,
                missingOfficialRatingCount,
                noMatchingRuleCount);

            return new RatingStandardizerBatchRunResult(true, false, totalCount, matchedCount, updatedCount, alreadyStandardizedCount, missingOfficialRatingCount, noMatchingRuleCount);
        }
        finally
        {
            RunLock.Release();
        }
    }

    public static async Task<RatingStandardizerRollbackRunResult> RollbackAsync(IProgress<double>? progress, CancellationToken cancellationToken, RatingRollbackSelection? selection = null)
    {
        var libraryManager = _libraryManager ?? throw new InvalidOperationException("Batch runner is not initialized.");
        var logger = _logger ?? throw new InvalidOperationException("Batch runner logger is not initialized.");

        if (!await RunLock.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Rating standardization is already running.");
        }

        try
        {
            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                logger.LogWarning("Plugin configuration is unavailable; skipping rating rollback.");
                progress?.Report(100);
                return RatingStandardizerRollbackRunResult.ConfigurationUnavailable;
            }

            if (!configuration.IsEnabled)
            {
                logger.LogInformation("Rating Standardizer is disabled. Rollback skipped.");
                progress?.Report(100);
                return RatingStandardizerRollbackRunResult.Disabled;
            }

            var items = libraryManager
                .GetUserRootFolder()
                .GetRecursiveChildren()
                .Concat(libraryManager.RootFolder.GetRecursiveChildren())
                .Concat(libraryManager.GetItemList(new InternalItemsQuery { MediaTypes = [global::Jellyfin.Data.Enums.MediaType.Video], Recursive = true }))
                .Where(static item => item is Video or MediaBrowser.Controller.Entities.TV.Series)
                .GroupBy(static item => item.Id)
                .Select(static group => group.First())
                .ToList();

            IReadOnlyCollection<string> targetLibraryIds = selection?.LibraryIds is { Count: > 0 }
                ? selection.LibraryIds
                : selection?.UseConfiguredTargetLibraries == false
                    ? Array.Empty<string>()
                    : configuration.TargetLibraryIds;
            var targetLookup = LibraryFilter.CreateTargetLookup(targetLibraryIds);
            if (targetLookup is not null)
            {
                var filteredItems = items.Where(item => LibraryFilter.IsMatch(item, targetLookup)).ToList();
                if (filteredItems.Count > 0)
                {
                    items = filteredItems;
                }
                else
                {
                    logger.LogWarning(
                        "Target library filter matched no items. Running rollback unfiltered so available movies and series can be restored. TargetLibraryCount={TargetLibraryCount}",
                        configuration.TargetLibraryIds.Count);
                }
            }

            var itemLookup = CreateItemLookup(selection?.ItemIds);
            var updatedAtLookup = CreateUpdatedAtLookup(selection?.UpdatedAtKeys);
            if (updatedAtLookup is not null)
            {
                var updatedItemIds = (Plugin.HistoryStore?.GetAll() ?? [])
                    .Where(entry => updatedAtLookup.Contains(UpdatedAtKey(entry.UpdatedAt)))
                    .Select(static entry => entry.ItemId)
                    .ToList();
                itemLookup = CreateItemLookup(updatedItemIds) ?? new HashSet<string>(StringComparer.Ordinal);
            }

            if (itemLookup is not null)
            {
                items = items.Where(item => itemLookup.Contains(NormalizeId(item.Id.ToString("N")))).ToList();
            }

            var restoredCount = 0;
            var alreadyOriginalCount = 0;
            var missingHistoryCount = 0;

            for (var index = 0; index < items.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items[index];
                var result = ItemRatingStandardizer.Rollback(item, Plugin.HistoryStore);
                if (result.Status == ItemRatingRollbackStatus.MissingHistory)
                {
                    missingHistoryCount++;
                }
                else if (result.Status == ItemRatingRollbackStatus.AlreadyOriginal)
                {
                    alreadyOriginalCount++;
                }

                if (result.RequiresSave)
                {
                    var parent = item.GetParent() ?? item;
                    await libraryManager.UpdateItemAsync(item, parent, ItemUpdateType.MetadataEdit, cancellationToken).ConfigureAwait(false);
                    restoredCount++;
                }

                if (items.Count > 0)
                {
                    progress?.Report(((double)(index + 1) / items.Count) * 100);
                }
            }

            if (items.Count == 0)
            {
                progress?.Report(100);
            }

            logger.LogInformation(
                "Rating Standardizer rollback completed. Scanned {ScannedCount} items, restored {RestoredCount}, already original {AlreadyOriginalCount}, missing history {MissingHistoryCount}.",
                items.Count,
                restoredCount,
                alreadyOriginalCount,
                missingHistoryCount);

            return new RatingStandardizerRollbackRunResult(true, false, items.Count, restoredCount, alreadyOriginalCount, missingHistoryCount);
        }
        finally
        {
            RunLock.Release();
        }
    }

    private static HashSet<string>? CreateItemLookup(IReadOnlyCollection<string>? itemIds)
    {
        if (itemIds is null || itemIds.Count == 0)
        {
            return null;
        }

        var lookup = itemIds
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id => NormalizeId(id))
            .Where(static id => id.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        return lookup.Count > 0 ? lookup : null;
    }

    private static HashSet<string>? CreateUpdatedAtLookup(IReadOnlyCollection<string>? updatedAtKeys)
    {
        if (updatedAtKeys is null || updatedAtKeys.Count == 0)
        {
            return null;
        }

        var lookup = updatedAtKeys
            .Where(static key => !string.IsNullOrWhiteSpace(key))
            .Select(static key => key.Trim())
            .ToHashSet(StringComparer.Ordinal);

        return lookup.Count > 0 ? lookup : null;
    }

    private static string UpdatedAtKey(DateTimeOffset updatedAt)
    {
        return updatedAt.UtcDateTime.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
    }

    private static string NormalizeId(string? id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? string.Empty
            : id.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }
}

internal readonly record struct RatingStandardizerBatchRunResult(
    bool Success,
    bool SkippedBecauseDisabled,
    int ScannedCount,
    int MatchedCount,
    int UpdatedCount,
    int AlreadyStandardizedCount,
    int MissingOfficialRatingCount,
    int NoMatchingRuleCount)
{
    public static RatingStandardizerBatchRunResult ConfigurationUnavailable => new(false, false, 0, 0, 0, 0, 0, 0);

    public static RatingStandardizerBatchRunResult Disabled => new(true, true, 0, 0, 0, 0, 0, 0);
}

internal readonly record struct RatingStandardizerRollbackRunResult(
    bool Success,
    bool SkippedBecauseDisabled,
    int ScannedCount,
    int RestoredCount,
    int AlreadyOriginalCount,
    int MissingHistoryCount)
{
    public static RatingStandardizerRollbackRunResult ConfigurationUnavailable => new(false, false, 0, 0, 0, 0);

    public static RatingStandardizerRollbackRunResult Disabled => new(true, true, 0, 0, 0, 0);
}
