using System;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

/// <summary>
/// Subscribes to Emby library events and standardizes ratings as items are added or updated.
/// </summary>
public sealed class ServerEntryPoint : IServerEntryPoint, IDisposable
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger _logger;
    private readonly RatingConverter _ratingConverter;
    private bool _isRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEntryPoint"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public ServerEntryPoint(ILibraryManager libraryManager, ILogManager logManager)
    {
        _libraryManager = libraryManager;
        _logger = logManager.GetLogger(GetType().FullName ?? nameof(ServerEntryPoint));
        _ratingConverter = new RatingConverter();
        RatingStandardizerBatchRunner.Initialize(libraryManager, _logger);
    }

    /// <inheritdoc />
    public void Run()
    {
        if (_isRegistered)
        {
            return;
        }

        _libraryManager.ItemAdded += OnItemChanged;
        _libraryManager.ItemUpdated += OnItemChanged;
        _isRegistered = true;

        _logger.Info("Rating Standardizer Emby entry point started.");
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

        _logger.Info("Rating Standardizer Emby entry point stopped.");
    }

    private void OnItemChanged(object? sender, ItemChangeEventArgs e)
    {
        try
        {
            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                _logger.Warn("Plugin configuration is unavailable; skipping rating standardization.");
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
                _logger.Debug("Rating mapping matched for {0}, but no save was required.", e.Item.Name);
                return;
            }

            var parent = e.Parent ?? e.Item.GetParent() ?? e.Item;
            _libraryManager.UpdateItem(e.Item, parent, ItemUpdateType.MetadataEdit);

            _logger.Info(
                "Standardized OfficialRating for {0} from {1} to {2}. Lock added: {3}",
                e.Item.Name,
                result.OriginalRating,
                result.TargetRating,
                result.LockAdded);
        }
        catch (Exception ex)
        {
            _logger.ErrorException("Failed to standardize rating for item {0}.", ex, e.Item?.Name ?? string.Empty);
        }
    }
}
