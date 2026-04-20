using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RatingStandardizer.Core;

namespace RatingStandardizer.Jellyfin;

/// <summary>
/// Subscribes to library events and standardizes ratings as items are added or updated.
/// </summary>
public sealed class ServerEntryPoint : IHostedService, IDisposable
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<ServerEntryPoint> _logger;
    private readonly RatingConverter _ratingConverter;
    private bool _isRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEntryPoint"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public ServerEntryPoint(ILibraryManager libraryManager, ILogger<ServerEntryPoint> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _ratingConverter = new RatingConverter();
        RatingStandardizerBatchRunner.Initialize(libraryManager, logger);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isRegistered)
        {
            return Task.CompletedTask;
        }

        _libraryManager.ItemAdded += OnItemChanged;
        _libraryManager.ItemUpdated += OnItemChanged;
        _isRegistered = true;

        _logger.LogInformation("Rating Standardizer Jellyfin entry point started.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_isRegistered)
        {
            return;
        }

        _libraryManager.ItemAdded -= OnItemChanged;
        _libraryManager.ItemUpdated -= OnItemChanged;
        _isRegistered = false;

        _logger.LogInformation("Rating Standardizer Jellyfin entry point stopped.");
    }

    private void OnItemChanged(object? sender, ItemChangeEventArgs e)
    {
        _ = ProcessItemChangeAsync(e);
    }

    private async Task ProcessItemChangeAsync(ItemChangeEventArgs e)
    {
        try
        {
            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                _logger.LogWarning("Plugin configuration is unavailable; skipping rating standardization.");
                return;
            }

            if (!configuration.IsEnabled)
            {
                return;
            }

            var result = ItemRatingStandardizer.Apply(e.Item, configuration.Mappings, _ratingConverter);
            if (!result.MatchedMapping)
            {
                return;
            }

            if (!result.RequiresSave)
            {
                _logger.LogDebug(
                    "Rating mapping matched for {ItemName}, but no save was required.",
                    e.Item.Name);
                return;
            }

            var parent = e.Parent ?? e.Item.GetParent() ?? e.Item;
            await _libraryManager.UpdateItemAsync(e.Item, parent, ItemUpdateType.MetadataEdit, CancellationToken.None).ConfigureAwait(false);

            _logger.LogInformation(
                "Standardized OfficialRating for {ItemName} from {OriginalRating} to {TargetRating}. Lock added: {LockAdded}",
                e.Item.Name,
                result.OriginalRating,
                result.TargetRating,
                result.LockAdded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to standardize rating for item {ItemName}.", e.Item?.Name);
        }
    }
}
