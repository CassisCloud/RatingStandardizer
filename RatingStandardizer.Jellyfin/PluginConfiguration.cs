using System.Collections.Generic;
using MediaBrowser.Model.Plugins;
using RatingStandardizer.Core;

namespace RatingStandardizer.Jellyfin;

/// <summary>
/// Stores Jellyfin plugin configuration.
/// </summary>
public sealed class PluginConfiguration : BasePluginConfiguration, IRatingStandardizerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the configured rating mappings.
    /// </summary>
    public List<RatingMapping> Mappings { get; set; } = RatingPresets.CreateJapanMappings();

    IReadOnlyList<RatingMapping> IRatingStandardizerConfiguration.Mappings => Mappings;
}
