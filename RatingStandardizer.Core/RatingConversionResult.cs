namespace RatingStandardizer.Core;

/// <summary>
/// Describes the result of a rating conversion attempt.
/// </summary>
public readonly struct RatingConversionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RatingConversionResult"/> struct.
    /// </summary>
    /// <param name="matchedMapping">Whether a mapping matched.</param>
    /// <param name="originalRating">The normalized original rating.</param>
    /// <param name="targetRating">The normalized target rating.</param>
    public RatingConversionResult(bool matchedMapping, string? originalRating, string? targetRating)
    {
        MatchedMapping = matchedMapping;
        OriginalRating = originalRating;
        TargetRating = targetRating;
    }

    /// <summary>
    /// Gets a value indicating whether a mapping matched.
    /// </summary>
    public bool MatchedMapping { get; }

    /// <summary>
    /// Gets the normalized original rating.
    /// </summary>
    public string? OriginalRating { get; }

    /// <summary>
    /// Gets the normalized target rating.
    /// </summary>
    public string? TargetRating { get; }

    /// <summary>
    /// Gets an empty conversion result.
    /// </summary>
    public static RatingConversionResult NoMatch => new(false, null, null);
}
