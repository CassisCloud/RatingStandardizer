using System.Collections.Generic;

namespace RatingStandardizer.Core;

/// <summary>
/// Exposes the configuration needed by the rating standardizer.
/// </summary>
public interface IRatingStandardizerConfiguration
{
    /// <summary>
    /// Gets a value indicating whether the plugin is enabled.
    /// </summary>
    bool IsEnabled { get; }

    string PresetId { get; }

    AmbiguousMappingPolicy AmbiguousMappingPolicy { get; }

    UnknownRatingBehavior UnknownRatingBehavior { get; }

    OriginalRatingStorageMode OriginalRatingStorageMode { get; }

    IReadOnlyList<RatingRule> CustomRules { get; }

    IReadOnlyList<RatingRenderPreset> CustomPresets { get; }

    IReadOnlyList<LibraryRatingProfile> LibraryProfiles { get; }
}
