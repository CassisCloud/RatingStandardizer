using System.Collections.Generic;

namespace RatingStandardizer.Core;

/// <summary>
/// Provides preset rating mappings.
/// </summary>
public static class RatingPresets
{
    /// <summary>
    /// Creates the default Japanese Eirin mapping set.
    /// </summary>
    /// <returns>A new list of rating mappings.</returns>
    public static List<RatingMapping> CreateJapanMappings()
    {
        return
        [
            new RatingMapping { OriginalRating = "6", TargetRating = "G" },
            new RatingMapping { OriginalRating = "6+", TargetRating = "G" },
            new RatingMapping { OriginalRating = "G", TargetRating = "G" },
            new RatingMapping { OriginalRating = "PG", TargetRating = "PG12" },
            new RatingMapping { OriginalRating = "12", TargetRating = "PG12" },
            new RatingMapping { OriginalRating = "12+", TargetRating = "PG12" },
            new RatingMapping { OriginalRating = "PG-13", TargetRating = "PG12" },
            new RatingMapping { OriginalRating = "14", TargetRating = "PG12" },
            new RatingMapping { OriginalRating = "TV-14", TargetRating = "PG12" },
            new RatingMapping { OriginalRating = "R", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "15", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "16", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "16+", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "AT-14", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "M", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "MA15+", TargetRating = "R15+" },
            new RatingMapping { OriginalRating = "NC-17", TargetRating = "R18+" },
            new RatingMapping { OriginalRating = "18+", TargetRating = "R18+" },
            new RatingMapping { OriginalRating = "PL-18", TargetRating = "R18+" },
            new RatingMapping { OriginalRating = "Unrated", TargetRating = "NR" },
            new RatingMapping { OriginalRating = "Not Rated", TargetRating = "NR" },
            new RatingMapping { OriginalRating = "NR", TargetRating = "NR" }
        ];
    }
}
