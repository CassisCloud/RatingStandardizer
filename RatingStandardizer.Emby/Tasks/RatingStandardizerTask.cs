using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby.Tasks;

public sealed class RatingStandardizerTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogManager _logManager;
    private readonly RatingConverter _ratingConverter;
    private static bool _isInitialized;

    public RatingStandardizerTask(ILibraryManager libraryManager, ILogManager logManager)
    {
        _libraryManager = libraryManager;
        _logManager = logManager;
        _ratingConverter = new RatingConverter();

        if (!_isInitialized)
        {
            RatingStandardizerBatchRunner.Initialize(libraryManager, logManager.GetLogger(nameof(RatingStandardizerBatchRunner)));
            _isInitialized = true;
        }
    }

    public string Name => "Standardize Official Ratings";

    public string Key => "RatingStandardizerBatchTask";

    public string Description => "Apply configured official rating mappings to existing movies and series.";

    public string Category => "Library";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return Array.Empty<TaskTriggerInfo>();
    }

    public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
    {
        var logger = _logManager.GetLogger(GetType().FullName ?? nameof(RatingStandardizerTask));

        var configuration = Plugin.Instance?.Configuration;
        if (configuration is null)
        {
            logger.Warn("Plugin configuration is unavailable; skipping scheduled rating standardization.");
            progress.Report(100);
            return Task.CompletedTask;
        }

        if (!configuration.IsEnabled)
        {
            logger.Info("Rating Standardizer is disabled. Scheduled task skipped.");
            progress.Report(100);
            return Task.CompletedTask;
        }

        var items = _libraryManager
            .RootFolder
            .GetRecursiveChildren()
            .Where(static item => item is Video || item is Series)
            .ToList();

        var totalCount = items.Count;
        var updatedCount = 0;
        var matchedCount = 0;

        for (var index = 0; index < totalCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = items[index];
            var result = ItemRatingStandardizer.Apply(item, configuration.Mappings, _ratingConverter);
            if (result.MatchedMapping)
            {
                matchedCount++;
            }

            if (result.RequiresSave)
            {
                var parent = item.GetParent() ?? item;
                _libraryManager.UpdateItem(item, parent, ItemUpdateType.MetadataEdit);
                updatedCount++;
            }

            if (totalCount > 0)
            {
                progress.Report(((double)(index + 1) / totalCount) * 100);
            }
        }

        if (totalCount == 0)
        {
            progress.Report(100);
        }

        logger.Info(
            "Rating Standardizer task completed. Scanned {0} items, matched {1}, updated {2}.",
            totalCount,
            matchedCount,
            updatedCount);

        return Task.CompletedTask;
    }
}