using System;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using RatingStandardizer.Core;

namespace RatingStandardizer.Emby;

internal static class ItemRatingStandardizer
{
    private const string OriginalRatingTagPrefix = "OriginalRating:";

    public static ItemRatingRollbackResult Rollback(BaseItem item, IRatingHistoryStore? historyStore)
    {
        if (item is not Video && item is not Series)
        {
            return ItemRatingRollbackResult.UnsupportedItemType;
        }

        var history = historyStore?.Get(item.Id.ToString("N"));
        var originalRating = history?.OriginalOfficialRating;
        var tags = item.Tags ?? Array.Empty<string>();
        var originalRatingTag = tags.FirstOrDefault(tag => tag.StartsWith(OriginalRatingTagPrefix, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(originalRating) && !string.IsNullOrWhiteSpace(originalRatingTag))
        {
            originalRating = originalRatingTag.Substring(OriginalRatingTagPrefix.Length).Trim();
        }

        if (string.IsNullOrWhiteSpace(originalRating))
        {
            return ItemRatingRollbackResult.MissingHistory;
        }

        var officialRatingChanged = !string.Equals(item.OfficialRating ?? string.Empty, originalRating, StringComparison.Ordinal);
        if (officialRatingChanged)
        {
            item.OfficialRating = originalRating;
        }

        var customRatingChanged = false;
        if (history is not null)
        {
            customRatingChanged = !string.Equals(item.CustomRating ?? string.Empty, history.OriginalCustomRating ?? string.Empty, StringComparison.Ordinal);
            if (customRatingChanged)
            {
                item.CustomRating = string.IsNullOrWhiteSpace(history.OriginalCustomRating) ? null : history.OriginalCustomRating;
            }
        }

        var cleanedTags = tags.Where(tag => !tag.StartsWith(OriginalRatingTagPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        var tagRemoved = cleanedTags.Length != tags.Length;
        if (tagRemoved)
        {
            item.Tags = cleanedTags;
        }

        var lockedFields = item.LockedFields ?? Array.Empty<MetadataFields>();
        var updatedLockedFields = lockedFields.Where(field => field != MetadataFields.OfficialRating).ToArray();
        var lockRemoved = updatedLockedFields.Length != lockedFields.Length;
        if (lockRemoved)
        {
            item.LockedFields = updatedLockedFields;
        }

        return officialRatingChanged || customRatingChanged || tagRemoved || lockRemoved
            ? ItemRatingRollbackResult.Restored
            : ItemRatingRollbackResult.AlreadyOriginal;
    }

    public static ItemRatingStandardizationResult Apply(BaseItem item, IRatingStandardizerConfiguration configuration, RatingConverter converter, IRatingHistoryStore? historyStore, string pluginVersion)
    {
        if (item is not Video && item is not Series)
        {
            return ItemRatingStandardizationResult.UnsupportedItemType;
        }

        if (string.IsNullOrWhiteSpace(item.OfficialRating))
        {
            return ItemRatingStandardizationResult.MissingOfficialRating;
        }

        var options = new RatingConversionOptions
        {
            PresetId = configuration.PresetId,
            AmbiguousMappingPolicy = configuration.AmbiguousMappingPolicy,
            UnknownRatingBehavior = configuration.UnknownRatingBehavior,
            OriginalRatingStorageMode = configuration.OriginalRatingStorageMode,
            CustomRules = configuration.CustomRules,
            CustomPresets = configuration.CustomPresets
        };

        var conversion = converter.Convert(item.OfficialRating, options);
        if (!conversion.MatchedMapping)
        {
            return ItemRatingStandardizationResult.NoMatchingRule;
        }

        var originalRating = item.OfficialRating;
        var ratingChanged = !string.Equals(item.OfficialRating ?? string.Empty, conversion.TargetRating ?? string.Empty, StringComparison.Ordinal);
        if (ratingChanged)
        {
            item.OfficialRating = string.IsNullOrEmpty(conversion.TargetRating) ? null : conversion.TargetRating;
        }

        var historySaved = SaveOriginalRatingHistory(item, originalRating, conversion, configuration, historyStore, pluginVersion);
        var tagAdded = AddOriginalRatingTag(item, originalRating, configuration.OriginalRatingStorageMode);
        var lockedFields = item.LockedFields ?? Array.Empty<MetadataFields>();
        var lockAdded = !lockedFields.Contains(MetadataFields.OfficialRating);
        if (lockAdded)
        {
            item.LockedFields = [.. lockedFields, MetadataFields.OfficialRating];
        }

        if (!ratingChanged && !lockAdded && !tagAdded && !historySaved)
        {
            return new ItemRatingStandardizationResult(true, false, false, false, false, ItemRatingStandardizationStatus.AlreadyStandardized, conversion.OriginalRating, conversion.TargetRating);
        }

        return new ItemRatingStandardizationResult(true, ratingChanged, lockAdded, tagAdded, historySaved, ItemRatingStandardizationStatus.Standardized, conversion.OriginalRating, conversion.TargetRating);
    }

    private static bool SaveOriginalRatingHistory(BaseItem item, string? originalRating, RatingConversionResult conversion, IRatingStandardizerConfiguration configuration, IRatingHistoryStore? historyStore, string pluginVersion)
    {
        if (!ShouldStoreInPluginStore(configuration.OriginalRatingStorageMode) || historyStore is null || string.IsNullOrWhiteSpace(originalRating))
        {
            return false;
        }

        var itemId = item.Id.ToString("N");
        var existing = historyStore.Get(itemId);
        var entry = existing ?? new RatingHistoryEntry
        {
            ItemId = itemId,
            OriginalOfficialRating = originalRating.Trim(),
            OriginalCustomRating = item.CustomRating ?? string.Empty
        };

        entry.Path = item.Path ?? string.Empty;
        entry.Name = item.Name ?? string.Empty;
        entry.ProductionYear = item.ProductionYear;
        entry.ProviderIds = item.ProviderIds is null ? [] : item.ProviderIds.ToDictionary(pair => pair.Key, pair => pair.Value);
        entry.OriginalSystem = conversion.System ?? string.Empty;
        entry.NormalizedAge = conversion.MinimumAge;
        entry.FinalRating = conversion.TargetRating ?? string.Empty;
        entry.PresetId = configuration.PresetId;
        entry.MappingMode = configuration.AmbiguousMappingPolicy.ToString();
        entry.MatchedRuleId = conversion.MatchedRuleId ?? string.Empty;
        entry.MatchedRuleName = conversion.MatchedRuleName ?? string.Empty;
        entry.PluginVersion = pluginVersion;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        historyStore.Save(entry);
        return existing is null;
    }

    private static bool AddOriginalRatingTag(BaseItem item, string? originalRating, OriginalRatingStorageMode storageMode)
    {
        if (!ShouldMirrorToTags(storageMode) || string.IsNullOrWhiteSpace(originalRating))
        {
            return false;
        }

        var tags = item.Tags ?? Array.Empty<string>();
        if (tags.Any(tag => tag.StartsWith(OriginalRatingTagPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        item.Tags = [.. tags, OriginalRatingTagPrefix + originalRating.Trim()];
        return true;
    }

    private static bool ShouldStoreInPluginStore(OriginalRatingStorageMode storageMode)
    {
        return storageMode is OriginalRatingStorageMode.PluginStoreOnly or OriginalRatingStorageMode.PluginStoreAndTags;
    }

    private static bool ShouldMirrorToTags(OriginalRatingStorageMode storageMode)
    {
        return storageMode is OriginalRatingStorageMode.TagsOnly or OriginalRatingStorageMode.PluginStoreAndTags or OriginalRatingStorageMode.Tags;
    }
}

internal readonly record struct ItemRatingStandardizationResult(
    bool MatchedMapping,
    bool RatingChanged,
    bool LockAdded,
    bool TagAdded,
    bool HistorySaved,
    ItemRatingStandardizationStatus Status,
    string? OriginalRating,
    string? TargetRating)
{
    public static ItemRatingStandardizationResult UnsupportedItemType => new(false, false, false, false, false, ItemRatingStandardizationStatus.UnsupportedItemType, null, null);

    public static ItemRatingStandardizationResult MissingOfficialRating => new(false, false, false, false, false, ItemRatingStandardizationStatus.MissingOfficialRating, null, null);

    public static ItemRatingStandardizationResult NoMatchingRule => new(false, false, false, false, false, ItemRatingStandardizationStatus.NoMatchingRule, null, null);

    public bool RequiresSave => RatingChanged || LockAdded || TagAdded;
}

internal enum ItemRatingStandardizationStatus
{
    UnsupportedItemType,
    MissingOfficialRating,
    NoMatchingRule,
    AlreadyStandardized,
    Standardized
}

internal readonly record struct ItemRatingRollbackResult(ItemRatingRollbackStatus Status)
{
    public static ItemRatingRollbackResult UnsupportedItemType => new(ItemRatingRollbackStatus.UnsupportedItemType);

    public static ItemRatingRollbackResult MissingHistory => new(ItemRatingRollbackStatus.MissingHistory);

    public static ItemRatingRollbackResult AlreadyOriginal => new(ItemRatingRollbackStatus.AlreadyOriginal);

    public static ItemRatingRollbackResult Restored => new(ItemRatingRollbackStatus.Restored);

    public bool RequiresSave => Status == ItemRatingRollbackStatus.Restored;
}

internal enum ItemRatingRollbackStatus
{
    UnsupportedItemType,
    MissingHistory,
    AlreadyOriginal,
    Restored
}
