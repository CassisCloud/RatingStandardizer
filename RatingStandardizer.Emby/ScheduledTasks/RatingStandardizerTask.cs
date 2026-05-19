using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace RatingStandardizer.Emby.ScheduledTasks;

public class RatingStandardizerTask : IScheduledTask
{
    private readonly ILogManager _logManager;

    public RatingStandardizerTask(ILibraryManager libraryManager, ILogManager logManager)
    {
        _logManager = logManager;
        RatingStandardizerBatchRunner.Initialize(libraryManager, logManager.GetLogger(nameof(RatingStandardizerBatchRunner)));
    }

    public string Name => "Rating Standardizer: Standardize Ratings";

    public string Key => "RatingStandardizer.StandardizeRatings";

    public string Description => "Normalize official ratings to the configured output preset for existing movies and series.";

    public string Category => "Rating Standardizer";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromDays(7).Ticks
            }
        ];
    }

    public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
    {
        var logger = _logManager.GetLogger(GetType().FullName ?? nameof(RatingStandardizerTask));

        var result = RatingStandardizerBatchRunner.Run(cancellationToken);
        progress.Report(100);

        if (result.SkippedBecauseDisabled)
        {
            logger.Info("Rating Standardizer task skipped because the plugin is disabled.");
        }

        logger.Info(
            "Rating Standardizer task completed. Scanned {0} items, matched {1}, updated {2}.",
            result.ScannedCount,
            result.MatchedCount,
            result.UpdatedCount);

        return Task.CompletedTask;
    }
}
