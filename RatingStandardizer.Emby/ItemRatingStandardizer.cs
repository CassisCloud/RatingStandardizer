using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

/// <summary>
/// Applies converted ratings to Emby items.
/// </summary>
internal static class ItemRatingStandardizer
{
    public static ItemRatingStandardizationResult Apply(BaseItem item, IReadOnlyList<RatingMapping>? mappings, RatingConverter converter)
    {
        if (item is not Video && item is not Series)
        {
            return ItemRatingStandardizationResult.NotApplicable;
        }

        var conversion = converter.Convert(item.OfficialRating, mappings);
        if (!conversion.MatchedMapping)
        {
            return ItemRatingStandardizationResult.NotApplicable;
        }

        var ratingChanged = !string.Equals(item.OfficialRating, conversion.TargetRating, StringComparison.Ordinal);
        if (ratingChanged)
        {
            item.OfficialRating = conversion.TargetRating;
        }

        var lockedFields = item.LockedFields ?? Array.Empty<MetadataFields>();
        var lockAdded = !lockedFields.Contains(MetadataFields.OfficialRating);
        if (lockAdded)
        {
            item.LockedFields = [.. lockedFields, MetadataFields.OfficialRating];
        }

        return new ItemRatingStandardizationResult(true, ratingChanged, lockAdded, conversion.OriginalRating, conversion.TargetRating);
    }
}

internal readonly record struct ItemRatingStandardizationResult(
    bool MatchedMapping,
    bool RatingChanged,
    bool LockAdded,
    string? OriginalRating,
    string? TargetRating)
{
    public static ItemRatingStandardizationResult NotApplicable => new(false, false, false, null, null);

    public bool RequiresSave => RatingChanged || LockAdded;
}
