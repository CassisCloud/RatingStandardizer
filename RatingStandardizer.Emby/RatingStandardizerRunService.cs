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

    public int ScannedCount { get; set; }

    public int MatchedCount { get; set; }

    public int UpdatedCount { get; set; }
}

public sealed class RatingStandardizerRunService : IService
{
    public object Post(RunRatingStandardizer request)
    {
        var result = RatingStandardizerBatchRunner.Run(CancellationToken.None);

        return new RunRatingStandardizerResponse
        {
            Success = result.Success,
            SkippedBecauseDisabled = result.SkippedBecauseDisabled,
            ScannedCount = result.ScannedCount,
            MatchedCount = result.MatchedCount,
            UpdatedCount = result.UpdatedCount
        };
    }
}
