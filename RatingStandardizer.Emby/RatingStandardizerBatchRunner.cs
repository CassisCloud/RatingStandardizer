using System;
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
        _libraryManager ??= libraryManager;
        _logger ??= logger;
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
                .RootFolder
                .GetRecursiveChildren()
                .Where(static item => item is Video or MediaBrowser.Controller.Entities.TV.Series)
                .ToList();

            var targetLookup = LibraryFilter.CreateTargetLookup(configuration.TargetLibraryIds);
            if (targetLookup is not null)
            {
                items = items.Where(item => LibraryFilter.IsMatch(item, targetLookup)).ToList();
            }

            var totalCount = items.Count;
            var updatedCount = 0;
            var matchedCount = 0;

            for (var index = 0; index < totalCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items[index];
                var result = ItemRatingStandardizer.Apply(item, configuration.Mappings, RatingConverter);
                if (result.MatchedMapping)
                {
                    matchedCount++;
                }

                if (result.RequiresSave)
                {
                    var parent = item.GetParent() ?? item;
                    libraryManager.UpdateItem(item, parent, ItemUpdateType.MetadataEdit);
                    updatedCount++;
                }
            }

            logger.Info(
                "Rating Standardizer batch run completed. Scanned {0} items, matched {1}, updated {2}.",
                totalCount,
                matchedCount,
                updatedCount);

            return new RatingStandardizerBatchRunResult(true, false, totalCount, matchedCount, updatedCount);
        }
        finally
        {
            lock (SyncRoot)
            {
                _isRunning = 0;
            }
        }
    }
}

internal readonly record struct RatingStandardizerBatchRunResult(
    bool Success,
    bool SkippedBecauseDisabled,
    int ScannedCount,
    int MatchedCount,
    int UpdatedCount)
{
    public static RatingStandardizerBatchRunResult ConfigurationUnavailable => new(false, false, 0, 0, 0);

    public static RatingStandardizerBatchRunResult Disabled => new(true, true, 0, 0, 0);
}
