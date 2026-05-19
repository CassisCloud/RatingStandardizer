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

    public string PresetId { get; set; } = RatingCatalog.AgeBasedPresetId;

    public AmbiguousMappingPolicy AmbiguousMappingPolicy { get; set; } = AmbiguousMappingPolicy.Conservative;

    public UnknownRatingBehavior UnknownRatingBehavior { get; set; } = UnknownRatingBehavior.KeepOriginal;

    public OriginalRatingStorageMode OriginalRatingStorageMode { get; set; } = OriginalRatingStorageMode.PluginStoreAndTags;

    public List<RatingRule> CustomRules { get; set; } = [];

    public List<RatingRenderPreset> CustomPresets { get; set; } = [];

    public List<LibraryRatingProfile> LibraryProfiles { get; set; } = [];

    /// <summary>
    /// Gets or sets the target virtual folder identifiers.
    /// Empty means all libraries are targeted.
    /// </summary>
    public List<string> TargetLibraryIds { get; set; } = [];

    IReadOnlyList<RatingRule> IRatingStandardizerConfiguration.CustomRules => CustomRules;

    IReadOnlyList<RatingRenderPreset> IRatingStandardizerConfiguration.CustomPresets => CustomPresets;

    IReadOnlyList<LibraryRatingProfile> IRatingStandardizerConfiguration.LibraryProfiles => LibraryProfiles;
}
