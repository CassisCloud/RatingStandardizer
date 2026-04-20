using System.Collections.Generic;
using MediaBrowser.Model.Plugins;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

/// <summary>
/// Stores Emby plugin configuration.
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

    /// <summary>
    /// Gets or sets the target virtual folder identifiers.
    /// Empty means all libraries are targeted.
    /// </summary>
    public List<string> TargetLibraryIds { get; set; } = [];

    IReadOnlyList<RatingMapping> IRatingStandardizerConfiguration.Mappings => Mappings;
}
