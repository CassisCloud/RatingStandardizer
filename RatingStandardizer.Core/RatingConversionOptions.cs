using System.Collections.Generic;

namespace RatingStandardizer.Core;

public sealed class RatingConversionOptions
{
    public string PresetId { get; set; } = RatingCatalog.AgeBasedPresetId;

    public AmbiguousMappingPolicy AmbiguousMappingPolicy { get; set; } = AmbiguousMappingPolicy.Conservative;

    public UnknownRatingBehavior UnknownRatingBehavior { get; set; } = UnknownRatingBehavior.KeepOriginal;

    public OriginalRatingStorageMode OriginalRatingStorageMode { get; set; } = OriginalRatingStorageMode.Tags;

    public IReadOnlyList<RatingRule> CustomRules { get; set; } = [];

    public IReadOnlyList<RatingRenderPreset> CustomPresets { get; set; } = [];
}
