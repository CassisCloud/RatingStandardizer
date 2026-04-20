using System;
using System.Collections.Generic;

namespace RatingStandardizer.Core;

/// <summary>
/// Converts official ratings using configurable mapping rules.
/// </summary>
public sealed class RatingConverter
{
    /// <summary>
    /// Converts a source rating into its configured target rating.
    /// </summary>
    /// <param name="originalRating">The source rating.</param>
    /// <param name="mappings">The mapping list.</param>
    /// <returns>The conversion result.</returns>
    public RatingConversionResult Convert(string? originalRating, IReadOnlyList<RatingMapping>? mappings)
    {
        var normalizedOriginal = Normalize(originalRating);
        if (string.IsNullOrEmpty(normalizedOriginal) || mappings is null || mappings.Count == 0)
        {
            return RatingConversionResult.NoMatch;
        }

        foreach (var mapping in mappings)
        {
            var source = Normalize(mapping.OriginalRating);
            var target = Normalize(mapping.TargetRating);
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                continue;
            }

            if (!string.Equals(normalizedOriginal, source, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return new RatingConversionResult(true, normalizedOriginal, target);
        }

        return RatingConversionResult.NoMatch;
    }

    private static string Normalize(string? value)
    {
        if (value is null) {
            return string.Empty;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? string.Empty : normalized;
    }
}
