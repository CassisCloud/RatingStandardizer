using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using RatingStandardizer.Core;

namespace RatingStandardizer.Jellyfin.Tasks;

/// <summary>
/// Applies rating standardization to existing library items.
/// </summary>
public sealed class RatingStandardizerTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<RatingStandardizerTask> _logger;
    private readonly RatingConverter _ratingConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatingStandardizerTask"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public RatingStandardizerTask(ILibraryManager libraryManager, ILogger<RatingStandardizerTask> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _ratingConverter = new RatingConverter();
        RatingStandardizerBatchRunner.Initialize(libraryManager, logger);
    }

    /// <inheritdoc />
    public string Name => "Standardize Official Ratings";

    /// <inheritdoc />
    public string Key => "RatingStandardizerBatchTask";

    /// <inheritdoc />
    public string Description => "Apply configured official rating mappings to existing movies and series.";

    /// <inheritdoc />
    public string Category => "Library";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return Array.Empty<TaskTriggerInfo>();
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        await RatingStandardizerBatchRunner.RunAsync(progress, cancellationToken).ConfigureAwait(false);
    }
}
