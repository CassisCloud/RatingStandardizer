namespace RatingStandardizer.Core;

public static class RatingConfigurationResolver
{
    public static EffectiveRatingConfiguration Resolve(IRatingStandardizerConfiguration configuration, LibraryRatingProfile? profile)
    {
        return new EffectiveRatingConfiguration
        {
            IsEnabled = configuration.IsEnabled,
            PresetId = profile is null || string.IsNullOrWhiteSpace(profile.PresetId) ? configuration.PresetId : profile.PresetId,
            AmbiguousMappingPolicy = profile?.AmbiguousMappingPolicy ?? configuration.AmbiguousMappingPolicy,
            UnknownRatingBehavior = profile?.UnknownRatingBehavior ?? configuration.UnknownRatingBehavior,
            OriginalRatingStorageMode = profile?.OriginalRatingStorageMode ?? configuration.OriginalRatingStorageMode,
            CustomRules = configuration.CustomRules,
            CustomPresets = configuration.CustomPresets,
            LibraryProfiles = configuration.LibraryProfiles,
            AppliedLibraryId = profile?.LibraryId ?? string.Empty,
            AppliedLibraryName = profile?.LibraryName ?? string.Empty
        };
    }
}
