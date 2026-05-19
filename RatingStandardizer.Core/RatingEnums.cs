namespace RatingStandardizer.Core;

public enum AmbiguousMappingPolicy
{
    Conservative,
    Nearest,
    Lenient
}

public enum UnknownRatingBehavior
{
    KeepOriginal,
    ConvertToUnrated,
    HideRating
}

public enum OriginalRatingStorageMode
{
    None,
    PluginStoreOnly,
    PluginStoreAndTags,
    TagsOnly,
    Tags
}

public enum RatingRuleMatchType
{
    Alias,
    Regex
}
