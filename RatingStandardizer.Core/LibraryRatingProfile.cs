namespace RatingStandardizer.Core;

public sealed class LibraryRatingProfile
{
    public string LibraryId { get; set; } = string.Empty;

    public string LibraryName { get; set; } = string.Empty;

    public string PresetId { get; set; } = string.Empty;

    public AmbiguousMappingPolicy? AmbiguousMappingPolicy { get; set; }

    public UnknownRatingBehavior? UnknownRatingBehavior { get; set; }

    public OriginalRatingStorageMode? OriginalRatingStorageMode { get; set; }
}
