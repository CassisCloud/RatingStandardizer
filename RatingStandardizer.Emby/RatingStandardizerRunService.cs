using System.Threading;
using MediaBrowser.Model.Services;

namespace RatingStandardizer.Emby;

[Route("/Plugins/RatingStandardizer/RunNow", "POST")]
public sealed class RunRatingStandardizer : IReturn<RunRatingStandardizerResponse>
{
}

public sealed class RunRatingStandardizerResponse
{
    public bool Success { get; set; }

    public bool SkippedBecauseDisabled { get; set; }

    public bool SkippedBecauseNoMappings { get; set; }

    public int ScannedCount { get; set; }

    public int MatchedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int AlreadyStandardizedCount { get; set; }

    public int MissingOfficialRatingCount { get; set; }

    public int NoMatchingRuleCount { get; set; }
}

public sealed class RatingStandardizerRunService : IService
{
    public RatingStandardizerRunService(MediaBrowser.Controller.Library.ILibraryManager libraryManager, MediaBrowser.Model.Logging.ILogManager logManager)
    {
        RatingStandardizerBatchRunner.Initialize(libraryManager, logManager.GetLogger(nameof(RatingStandardizerBatchRunner)));
    }

    public object Post(RunRatingStandardizer request)
    {
        var result = RatingStandardizerBatchRunner.Run(CancellationToken.None);

        return new RunRatingStandardizerResponse
        {
            Success = result.Success,
            SkippedBecauseDisabled = result.SkippedBecauseDisabled,
            SkippedBecauseNoMappings = result.SkippedBecauseNoMappings,
            ScannedCount = result.ScannedCount,
            MatchedCount = result.MatchedCount,
            UpdatedCount = result.UpdatedCount,
            AlreadyStandardizedCount = result.AlreadyStandardizedCount,
            MissingOfficialRatingCount = result.MissingOfficialRatingCount,
            NoMatchingRuleCount = result.NoMatchingRuleCount
        };
    }
}
