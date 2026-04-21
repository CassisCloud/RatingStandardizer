using System;
using System.Collections.Generic;
using System.Text;

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
        var displayOriginal = NormalizeDisplayValue(originalRating);
        var normalizedOriginal = NormalizeForComparison(originalRating);
        if (string.IsNullOrEmpty(normalizedOriginal) || mappings is null || mappings.Count == 0)
        {
            return RatingConversionResult.NoMatch;
        }

        foreach (var mapping in mappings)
        {
            var source = NormalizeForComparison(mapping.OriginalRating);
            var target = NormalizeDisplayValue(mapping.TargetRating);
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                continue;
            }

            if (!string.Equals(normalizedOriginal, source, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return new RatingConversionResult(true, displayOriginal, target);
        }

        return RatingConversionResult.NoMatch;
    }

    private static string NormalizeDisplayValue(string? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? string.Empty : normalized;
    }

    private static string NormalizeForComparison(string? value)
    {
        var displayValue = NormalizeDisplayValue(value);
        if (displayValue.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(displayValue.Length);
        foreach (var character in displayValue)
        {
            if (char.IsLetterOrDigit(character) || character == '+')
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString();
    }
}
