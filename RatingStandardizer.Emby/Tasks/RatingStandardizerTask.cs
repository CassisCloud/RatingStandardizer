using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace RatingStandardizer.Emby.Tasks;

public sealed class RatingStandardizerTask : IScheduledTask
{
    private readonly ILogManager _logManager;

    public RatingStandardizerTask(ILibraryManager libraryManager, ILogManager logManager)
    {
        _logManager = logManager;
        RatingStandardizerBatchRunner.Initialize(libraryManager, logManager.GetLogger(nameof(RatingStandardizerBatchRunner)));
    }

    public string Name => "Standardize Official Ratings";

    public string Key => "RatingStandardizerBatchTask";

    public string Description => "Apply configured official rating mappings to existing movies and series.";

    public string Category => "Library";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo
            {
                Type = "WeeklyTrigger",
                DayOfWeek = DayOfWeek.Monday,
                TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
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
        else if (result.SkippedBecauseNoMappings)
        {
            logger.Warn("Rating Standardizer task skipped because no custom mappings are configured.");
        }

        logger.Info(
            "Rating Standardizer task completed. Scanned {0} items, matched {1}, updated {2}.",
            result.ScannedCount,
            result.MatchedCount,
            result.UpdatedCount);

        return Task.CompletedTask;
    }
}
