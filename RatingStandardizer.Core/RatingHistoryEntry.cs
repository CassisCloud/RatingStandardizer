using System;
using System.Collections.Generic;

namespace RatingStandardizer.Core;

public sealed class RatingHistoryEntry
{
    public string ItemId { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? ProductionYear { get; set; }

    public Dictionary<string, string> ProviderIds { get; set; } = [];

    public string OriginalOfficialRating { get; set; } = string.Empty;

    public string OriginalCustomRating { get; set; } = string.Empty;

    public string OriginalSystem { get; set; } = string.Empty;

    public int? NormalizedAge { get; set; }

    public string FinalRating { get; set; } = string.Empty;

    public string PresetId { get; set; } = string.Empty;

    public string MappingMode { get; set; } = string.Empty;

    public string MatchedRuleId { get; set; } = string.Empty;

    public string MatchedRuleName { get; set; } = string.Empty;

    public string PluginVersion { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
