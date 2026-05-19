using System.Collections.Generic;

namespace RatingStandardizer.Core;

public class RatingRule
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public RatingRuleMatchType MatchType { get; set; } = RatingRuleMatchType.Alias;

    public List<string> Aliases { get; set; } = [];

    public string Pattern { get; set; } = string.Empty;

    public int? MinimumAgeFromGroup { get; set; }

    public string NormalizedLabelTemplate { get; set; } = string.Empty;

    public int MinimumAge { get; set; }

    public int? ConservativeAge { get; set; }

    public int? LenientAge { get; set; }

    public string NormalizedLabel { get; set; } = string.Empty;

    public string System { get; set; } = "GENERIC";

    public string Severity { get; set; } = "unknown";

    public string Restriction { get; set; } = "unknown";

    public double Confidence { get; set; } = 1.0;

    public int Priority { get; set; }

    public bool Enabled { get; set; } = true;

    public bool IsCustom { get; set; }
}
