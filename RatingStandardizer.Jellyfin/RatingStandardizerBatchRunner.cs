using System;
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

            if (configuration.Mappings.Count == 0)
            {
                logger.LogWarning("Rating Standardizer has no mappings configured. Batch run skipped.");
                progress?.Report(100);
                return RatingStandardizerBatchRunResult.NoMappingsConfigured;
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
            var alreadyStandardizedCount = 0;
            var missingOfficialRatingCount = 0;
            var noMatchingRuleCount = 0;

            for (var index = 0; index < totalCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items[index];
                var result = ItemRatingStandardizer.Apply(item, configuration.Mappings, RatingConverter);
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

            return new RatingStandardizerBatchRunResult(true, false, false, totalCount, matchedCount, updatedCount, alreadyStandardizedCount, missingOfficialRatingCount, noMatchingRuleCount);
        }
        finally
        {
            RunLock.Release();
        }
    }
}

internal readonly record struct RatingStandardizerBatchRunResult(
    bool Success,
    bool SkippedBecauseDisabled,
    bool SkippedBecauseNoMappings,
    int ScannedCount,
    int MatchedCount,
    int UpdatedCount,
    int AlreadyStandardizedCount,
    int MissingOfficialRatingCount,
    int NoMatchingRuleCount)
{
    public static RatingStandardizerBatchRunResult ConfigurationUnavailable => new(false, false, false, 0, 0, 0, 0, 0, 0);

    public static RatingStandardizerBatchRunResult Disabled => new(true, true, false, 0, 0, 0, 0, 0, 0);

    public static RatingStandardizerBatchRunResult NoMappingsConfigured => new(true, false, true, 0, 0, 0, 0, 0, 0);
}
