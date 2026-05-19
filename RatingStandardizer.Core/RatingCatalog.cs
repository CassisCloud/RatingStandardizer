using System.Collections.Generic;

namespace RatingStandardizer.Core;

public static class RatingCatalog
{
    public const string AgeBasedPresetId = "age-based";

    public static List<RatingRule> CreateBuiltInRules()
    {
        return
        [
            Rule("generic_all", "All ages", ["ALL", "0+", "0", "G", "TV-G", "TV-Y", "U"], 0, "GENERIC", "general", "unrestricted", 100),
            Rule("generic_6", "Generic 6+", ["6", "6+", "FSK-6"], 6, "GENERIC", "child", "advisory", 100),
            Rule("us_tv_y7", "US TV-Y7", ["TV-Y7", "TVY7"], 7, "US_TV", "child", "advisory", 200),
            Rule("generic_10", "Generic 10+", ["10", "10+"], 10, "GENERIC", "older-child", "advisory", 100),
            Rule("generic_pg", "Parental guidance", ["PG", "TV-PG"], 10, "GENERIC", "older-child", "advisory", 90, conservativeAge: 12),
            Rule("generic_12", "Generic 12+", ["12", "12+", "12A", "PG12", "JP-PG12", "JP-12", "FSK-12", "BR-12", "CERO-B"], 12, "GENERIC", "teen", "advisory", 120),
            Rule("us_mpa_pg13", "US MPA PG-13", ["PG-13", "PG13"], 13, "US_MPA", "teen", "advisory", 200),
            Rule("generic_14", "Generic 14+", ["14", "14+", "TV-14", "AT-14", "BR-14"], 14, "GENERIC", "teen", "advisory", 120),
            Rule("generic_15", "Generic 15+", ["15", "15+", "R15+", "JP-R15+", "MA15+", "MA 15+", "CERO-C", "M"], 15, "GENERIC", "teen", "restricted", 120),
            Rule("generic_16", "Generic 16+", ["16", "16+", "FSK-16", "BR-16"], 16, "GENERIC", "mature", "restricted", 120),
            Rule("us_mpa_r", "US MPA R", ["R"], 17, "US_MPA", "mature", "restricted", 200, conservativeAge: 18),
            Rule("us_tv_ma", "US TV-MA", ["TV-MA", "TVMA"], 17, "US_TV", "mature", "mature", 200, conservativeAge: 18),
            Rule("generic_18", "Generic 18+", ["18", "18+", "+18", "R18+", "JP-R18+", "R 18+", "NC-17", "NC17", "FSK-18", "BR-18", "PL-18", "CERO-Z"], 18, "GENERIC", "adult", "adult", 120),
            Rule("unrated", "Unrated", ["NR", "N/R", "UNRATED", "NOT RATED"], 0, "GENERIC", "unknown", "unknown", 80)
        ];
    }

    public static List<RatingRenderPreset> CreateBuiltInPresets()
    {
        return
        [
            Preset(AgeBasedPresetId, "Age-based", [Range(0, 0, "ALL"), Range(1, 6, "6+"), Range(7, 10, "10+"), Range(11, 12, "12+"), Range(13, 14, "14+"), Range(15, 15, "15+"), Range(16, 17, "16+"), Range(18, 99, "18+")]),
            Preset("my-simple-ratings", "My Simple Ratings", [Range(0, 11, "ALL"), Range(12, 14, "TEEN"), Range(15, 17, "MATURE"), Range(18, 99, "ADULT")]),
            Preset("japan-eirin", "Japan / Eirin", [Range(0, 11, "G"), Range(12, 14, "PG12"), Range(15, 17, "R15+"), Range(18, 99, "R18+")]),
            Preset("us-tv", "United States / TV", [Range(0, 6, "TV-G"), Range(7, 7, "TV-Y7"), Range(8, 13, "TV-PG"), Range(14, 16, "TV-14"), Range(17, 99, "TV-MA")]),
            Preset("us-film", "United States / Film", [Range(0, 9, "G"), Range(10, 12, "PG"), Range(13, 16, "PG-13"), Range(17, 17, "R"), Range(18, 99, "NC-17")]),
            Preset("europe-generic", "Europe / Generic", [Range(0, 0, "0+"), Range(1, 6, "6+"), Range(7, 10, "10+"), Range(11, 12, "12+"), Range(13, 14, "14+"), Range(15, 15, "15+"), Range(16, 17, "16+"), Range(18, 99, "18+")]),
            Preset("uk-bbfc", "Europe / UK BBFC", [Range(0, 0, "U"), Range(1, 11, "PG"), Range(12, 14, "12"), Range(15, 17, "15"), Range(18, 99, "18")]),
            Preset("de-fsk", "Europe / Germany FSK", [Range(0, 0, "FSK-0"), Range(1, 6, "FSK-6"), Range(7, 12, "FSK-12"), Range(13, 16, "FSK-16"), Range(17, 99, "FSK-18")]),
            Preset("australia", "Australia", [Range(0, 0, "G"), Range(1, 10, "PG"), Range(11, 14, "M"), Range(15, 17, "MA15+"), Range(18, 99, "R18+")])
        ];
    }

    private static RatingRule Rule(string id, string name, List<string> aliases, int age, string system, string severity, string restriction, int priority, int? conservativeAge = null, int? lenientAge = null)
    {
        return new RatingRule
        {
            Id = id,
            Name = name,
            Aliases = aliases,
            MinimumAge = age,
            ConservativeAge = conservativeAge,
            LenientAge = lenientAge,
            NormalizedLabel = age == 0 ? "ALL" : age + "+",
            System = system,
            Severity = severity,
            Restriction = restriction,
            Priority = priority,
            Enabled = true
        };
    }

    private static RatingRenderPreset Preset(string id, string name, List<RatingRenderRule> render)
    {
        return new RatingRenderPreset { Id = id, Name = name, Render = render };
    }

    private static RatingRenderRule Range(int min, int max, string label)
    {
        return new RatingRenderRule { Min = min, Max = max, Label = label };
    }
}
