using System.Collections.Generic;

namespace RatingStandardizer.Core;

public sealed class EffectiveRatingConfiguration : IRatingStandardizerConfiguration
{
    public bool IsEnabled { get; set; }

    public string PresetId { get; set; } = RatingCatalog.AgeBasedPresetId;

    public AmbiguousMappingPolicy AmbiguousMappingPolicy { get; set; } = AmbiguousMappingPolicy.Conservative;

    public UnknownRatingBehavior UnknownRatingBehavior { get; set; } = UnknownRatingBehavior.KeepOriginal;

    public OriginalRatingStorageMode OriginalRatingStorageMode { get; set; } = OriginalRatingStorageMode.PluginStoreAndTags;

    public IReadOnlyList<RatingRule> CustomRules { get; set; } = [];

    public IReadOnlyList<RatingRenderPreset> CustomPresets { get; set; } = [];

    public IReadOnlyList<LibraryRatingProfile> LibraryProfiles { get; set; } = [];

    public string AppliedLibraryId { get; set; } = string.Empty;

    public string AppliedLibraryName { get; set; } = string.Empty;
}
