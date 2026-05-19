using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

/// <summary>
/// Runs batch rating standardization for existing library items.
/// </summary>
internal static class RatingStandardizerBatchRunner
{
    private static readonly object SyncRoot = new();
    private static ILibraryManager? _libraryManager;
    private static ILogger? _logger;
    private static readonly RatingConverter RatingConverter = new();
    private static int _isRunning;

    public static void Initialize(ILibraryManager libraryManager, ILogger logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public static RatingStandardizerBatchRunResult Run(CancellationToken cancellationToken)
    {
        var libraryManager = _libraryManager ?? throw new InvalidOperationException("Batch runner is not initialized.");
        var logger = _logger ?? throw new InvalidOperationException("Batch runner logger is not initialized.");

        lock (SyncRoot)
        {
            if (_isRunning == 1)
            {
                throw new InvalidOperationException("Rating standardization is already running.");
            }

            _isRunning = 1;
        }

        try
        {
            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                logger.Warn("Plugin configuration is unavailable; skipping batch rating standardization.");
                return RatingStandardizerBatchRunResult.ConfigurationUnavailable;
            }

            if (!configuration.IsEnabled)
            {
                logger.Info("Rating Standardizer is disabled. Batch run skipped.");
                return RatingStandardizerBatchRunResult.Disabled;
            }

            var items = libraryManager
                .GetUserRootFolder()
                .GetRecursiveChildren()
                .Concat(libraryManager.RootFolder.GetRecursiveChildren())
                .Concat(libraryManager.GetItemList(new InternalItemsQuery { MediaTypes = [MediaType.Video], Recursive = true }))
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
                    logger.Warn("Target library filter matched no items. Running unfiltered so Run Now still processes available movies and series. TargetLibraryCount={0}", configuration.TargetLibraryIds.Count);
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

                    logger.Debug(
                        "Skipped item {0}. OfficialRating={1}, Reason={2}.",
                        item.Name,
                        item.OfficialRating,
                        result.Status);
                }

                if (result.RequiresSave)
                {
                    var parent = item.GetParent() ?? item;
                    libraryManager.UpdateItem(item, parent, ItemUpdateType.MetadataEdit);
                    updatedCount++;
                }
            }

            logger.Info(
                "Rating Standardizer batch run completed. Scanned {0} items, matched {1}, updated {2}, already standardized {3}, missing official rating {4}, no matching rule {5}.",
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
            lock (SyncRoot)
            {
                _isRunning = 0;
            }
        }
    }

    public static RatingStandardizerRollbackRunResult Rollback(CancellationToken cancellationToken, RatingRollbackSelection? selection = null)
    {
        var libraryManager = _libraryManager ?? throw new InvalidOperationException("Batch runner is not initialized.");
        var logger = _logger ?? throw new InvalidOperationException("Batch runner logger is not initialized.");

        lock (SyncRoot)
        {
            if (_isRunning == 1)
            {
                throw new InvalidOperationException("Rating standardization is already running.");
            }

            _isRunning = 1;
        }

        try
        {
            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                logger.Warn("Plugin configuration is unavailable; skipping rating rollback.");
                return RatingStandardizerRollbackRunResult.ConfigurationUnavailable;
            }

            if (!configuration.IsEnabled)
            {
                logger.Info("Rating Standardizer is disabled. Rollback skipped.");
                return RatingStandardizerRollbackRunResult.Disabled;
            }

            var items = libraryManager
                .GetUserRootFolder()
                .GetRecursiveChildren()
                .Concat(libraryManager.RootFolder.GetRecursiveChildren())
                .Concat(libraryManager.GetItemList(new InternalItemsQuery { MediaTypes = [MediaType.Video], Recursive = true }))
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
                    logger.Warn("Target library filter matched no items. Running rollback unfiltered so available movies and series can be restored. TargetLibraryCount={0}", configuration.TargetLibraryIds.Count);
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

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

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
                    libraryManager.UpdateItem(item, parent, ItemUpdateType.MetadataEdit);
                    restoredCount++;
                }
            }

            logger.Info(
                "Rating Standardizer rollback completed. Scanned {0} items, restored {1}, already original {2}, missing history {3}.",
                items.Count,
                restoredCount,
                alreadyOriginalCount,
                missingHistoryCount);

            return new RatingStandardizerRollbackRunResult(true, false, items.Count, restoredCount, alreadyOriginalCount, missingHistoryCount);
        }
        finally
        {
            lock (SyncRoot)
            {
                _isRunning = 0;
            }
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
