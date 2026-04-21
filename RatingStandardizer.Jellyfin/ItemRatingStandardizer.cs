using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using RatingStandardizer.Core;

namespace RatingStandardizer.Jellyfin;

/// <summary>
/// Applies converted ratings to Jellyfin items.
/// </summary>
internal static class ItemRatingStandardizer
{
    public static ItemRatingStandardizationResult Apply(BaseItem item, IReadOnlyList<RatingMapping>? mappings, RatingConverter converter)
    {
        if (item is not Video && item is not Series)
        {
            return ItemRatingStandardizationResult.UnsupportedItemType;
        }

        if (mappings is null || mappings.Count == 0)
        {
            return ItemRatingStandardizationResult.NoMappingsConfigured;
        }

        if (string.IsNullOrWhiteSpace(item.OfficialRating))
        {
            return ItemRatingStandardizationResult.MissingOfficialRating;
        }

        var conversion = converter.Convert(item.OfficialRating, mappings);
        if (!conversion.MatchedMapping)
        {
            return ItemRatingStandardizationResult.NoMatchingRule;
        }

        var ratingChanged = !string.Equals(item.OfficialRating, conversion.TargetRating, StringComparison.Ordinal);
        if (ratingChanged)
        {
            item.OfficialRating = conversion.TargetRating;
        }

        var lockedFields = item.LockedFields ?? Array.Empty<MetadataField>();
        var lockAdded = !lockedFields.Contains(MetadataField.OfficialRating);
        if (lockAdded)
        {
            item.LockedFields = [.. lockedFields, MetadataField.OfficialRating];
        }

        if (!ratingChanged && !lockAdded)
        {
            return new ItemRatingStandardizationResult(true, false, false, ItemRatingStandardizationStatus.AlreadyStandardized, conversion.OriginalRating, conversion.TargetRating);
        }

        return new ItemRatingStandardizationResult(true, ratingChanged, lockAdded, ItemRatingStandardizationStatus.Standardized, conversion.OriginalRating, conversion.TargetRating);
    }
}

internal readonly record struct ItemRatingStandardizationResult(
    bool MatchedMapping,
    bool RatingChanged,
    bool LockAdded,
    ItemRatingStandardizationStatus Status,
    string? OriginalRating,
    string? TargetRating)
{
    public static ItemRatingStandardizationResult UnsupportedItemType => new(false, false, false, ItemRatingStandardizationStatus.UnsupportedItemType, null, null);

    public static ItemRatingStandardizationResult NoMappingsConfigured => new(false, false, false, ItemRatingStandardizationStatus.NoMappingsConfigured, null, null);

    public static ItemRatingStandardizationResult MissingOfficialRating => new(false, false, false, ItemRatingStandardizationStatus.MissingOfficialRating, null, null);

    public static ItemRatingStandardizationResult NoMatchingRule => new(false, false, false, ItemRatingStandardizationStatus.NoMatchingRule, null, null);

    public bool RequiresSave => RatingChanged || LockAdded;
}

internal enum ItemRatingStandardizationStatus
{
    UnsupportedItemType,
    NoMappingsConfigured,
    MissingOfficialRating,
    NoMatchingRule,
    AlreadyStandardized,
    Standardized
}
