namespace RatingStandardizer.Core;

/// <summary>
/// Represents a single rating mapping rule.
/// </summary>
public sealed class RatingMapping
{
    /// <summary>
    /// Gets or sets the source rating.
    /// </summary>
    public string OriginalRating { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target rating.
    /// </summary>
    public string TargetRating { get; set; } = string.Empty;
}
