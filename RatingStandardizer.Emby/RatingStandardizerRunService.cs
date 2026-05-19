using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediaBrowser.Model.Services;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

[Route("/Plugins/RatingStandardizer/RunNow", "POST")]
public sealed class RunRatingStandardizer : IReturn<RunRatingStandardizerResponse>
{
}

[Route("/Plugins/RatingStandardizer/Rollback", "POST")]
public sealed class RollbackRatingStandardizer : IReturn<RollbackRatingStandardizerResponse>
{
    public bool UseConfiguredTargetLibraries { get; set; } = true;

    public List<string> LibraryIds { get; set; } = [];

    public List<string> ItemIds { get; set; } = [];

    public List<string> UpdatedAtKeys { get; set; } = [];
}

[Route("/Plugins/RatingStandardizer/History", "GET")]
public sealed class GetRatingStandardizerHistory : IReturn<RatingHistoryResponse>
{
    public int Limit { get; set; } = 200;
}

[Route("/Plugins/RatingStandardizer/HistoryExport", "GET")]
public sealed class ExportRatingStandardizerHistory : IReturn<RatingHistoryExportResponse>
{
}

[Route("/Plugins/RatingStandardizer/HistoryImport", "POST")]
public sealed class ImportRatingStandardizerHistory : IReturn<RatingHistoryImportResponse>
{
    public List<RatingHistoryEntry> Items { get; set; } = [];
}

public sealed class RunRatingStandardizerResponse
{
    public bool Success { get; set; }

    public bool SkippedBecauseDisabled { get; set; }

    public int ScannedCount { get; set; }

    public int MatchedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int AlreadyStandardizedCount { get; set; }

    public int MissingOfficialRatingCount { get; set; }

    public int NoMatchingRuleCount { get; set; }
}

public sealed class RollbackRatingStandardizerResponse
{
    public bool Success { get; set; }

    public bool SkippedBecauseDisabled { get; set; }

    public int ScannedCount { get; set; }

    public int RestoredCount { get; set; }

    public int AlreadyOriginalCount { get; set; }

    public int MissingHistoryCount { get; set; }
}

public sealed class RatingHistoryResponse
{
    public List<RatingHistoryItemResponse> Items { get; set; } = [];
}

public sealed class RatingHistoryItemResponse
{
    public string ItemId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string OriginalOfficialRating { get; set; } = string.Empty;

    public string FinalRating { get; set; } = string.Empty;

    public string PresetId { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;
}

public sealed class RatingHistoryExportResponse
{
    public List<RatingHistoryEntry> Items { get; set; } = [];
}

public sealed class RatingHistoryImportResponse
{
    public int ImportedCount { get; set; }
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
            ScannedCount = result.ScannedCount,
            MatchedCount = result.MatchedCount,
            UpdatedCount = result.UpdatedCount,
            AlreadyStandardizedCount = result.AlreadyStandardizedCount,
            MissingOfficialRatingCount = result.MissingOfficialRatingCount,
            NoMatchingRuleCount = result.NoMatchingRuleCount
        };
    }

    public object Post(RollbackRatingStandardizer request)
    {
        var result = RatingStandardizerBatchRunner.Rollback(CancellationToken.None, new RatingRollbackSelection
        {
            UseConfiguredTargetLibraries = request.UseConfiguredTargetLibraries,
            LibraryIds = request.LibraryIds ?? [],
            ItemIds = request.ItemIds ?? [],
            UpdatedAtKeys = request.UpdatedAtKeys ?? []
        });

        return new RollbackRatingStandardizerResponse
        {
            Success = result.Success,
            SkippedBecauseDisabled = result.SkippedBecauseDisabled,
            ScannedCount = result.ScannedCount,
            RestoredCount = result.RestoredCount,
            AlreadyOriginalCount = result.AlreadyOriginalCount,
            MissingHistoryCount = result.MissingHistoryCount
        };
    }

    public object Get(GetRatingStandardizerHistory request)
    {
        var take = request.Limit <= 0 ? 200 : Math.Min(request.Limit, 1000);
        IReadOnlyList<RatingHistoryEntry> entries = Plugin.HistoryStore?.GetAll() ?? [];

        return new RatingHistoryResponse
        {
            Items = entries.Take(take).Select(static entry => new RatingHistoryItemResponse
            {
                ItemId = entry.ItemId,
                Name = entry.Name,
                Path = entry.Path,
                OriginalOfficialRating = entry.OriginalOfficialRating,
                FinalRating = entry.FinalRating,
                PresetId = entry.PresetId,
                UpdatedAt = entry.UpdatedAt.ToString("O")
            }).ToList()
        };
    }

    public object Get(ExportRatingStandardizerHistory request)
    {
        return new RatingHistoryExportResponse
        {
            Items = (Plugin.HistoryStore?.GetAll() ?? []).ToList()
        };
    }

    public object Post(ImportRatingStandardizerHistory request)
    {
        var entries = request.Items
            .Where(static entry => !string.IsNullOrWhiteSpace(entry.ItemId))
            .ToList();

        Plugin.HistoryStore?.SaveMany(entries);

        return new RatingHistoryImportResponse
        {
            ImportedCount = entries.Count
        };
    }
}
