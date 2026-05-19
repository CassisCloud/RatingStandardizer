using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RatingStandardizer.Core;

namespace RatingStandardizer.Jellyfin;

[ApiController]
[Route("Plugins/RatingStandardizer")]
public sealed class RatingStandardizerRunController : ControllerBase
{
    public RatingStandardizerRunController(ILibraryManager libraryManager, ILogger<RatingStandardizerRunController> logger)
    {
        RatingStandardizerBatchRunner.Initialize(libraryManager, logger);
    }

    [HttpPost("RunNow")]
    public async Task<RunRatingStandardizerResponse> RunNow(CancellationToken cancellationToken)
    {
        var result = await RatingStandardizerBatchRunner.RunAsync(null, cancellationToken).ConfigureAwait(false);

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

    [HttpPost("Rollback")]
    public async Task<RollbackRatingStandardizerResponse> Rollback([FromBody] RatingRollbackSelection? selection, CancellationToken cancellationToken)
    {
        var result = await RatingStandardizerBatchRunner.RollbackAsync(null, cancellationToken, selection).ConfigureAwait(false);

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

    [HttpGet("History")]
    public RatingHistoryResponse History([FromQuery] int limit = 200)
    {
        var take = limit <= 0 ? 200 : Math.Min(limit, 1000);
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

    [HttpGet("HistoryExport")]
    public RatingHistoryExportResponse HistoryExport()
    {
        return new RatingHistoryExportResponse
        {
            Items = (Plugin.HistoryStore?.GetAll() ?? []).ToList()
        };
    }

    [HttpPost("HistoryImport")]
    public RatingHistoryImportResponse HistoryImport([FromBody] RatingHistoryImportRequest request)
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

public sealed class RatingHistoryImportRequest
{
    public List<RatingHistoryEntry> Items { get; set; } = [];
}

public sealed class RatingHistoryImportResponse
{
    public int ImportedCount { get; set; }
}
