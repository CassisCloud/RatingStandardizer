using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RatingStandardizer.Core;

/// <summary>
/// Normalizes ratings to an internal minimum age, then renders that age through the selected preset.
/// </summary>
public sealed class RatingConverter
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private readonly IReadOnlyList<RatingRule> _builtInRules = RatingCatalog.CreateBuiltInRules();
    private readonly IReadOnlyList<RatingRenderPreset> _builtInPresets = RatingCatalog.CreateBuiltInPresets();

    public RatingConversionResult Convert(string? originalRating, RatingConversionOptions options)
    {
        var displayOriginal = NormalizeDisplayValue(originalRating);
        var normalizedOriginal = NormalizeForComparison(originalRating);
        if (string.IsNullOrEmpty(normalizedOriginal))
        {
            return RatingConversionResult.NoMatch;
        }

        options ??= new RatingConversionOptions();
        var match = FindRule(displayOriginal, normalizedOriginal, options.CustomRules, true) ?? FindRule(displayOriginal, normalizedOriginal, _builtInRules, false);
        if (match is null)
        {
            return HandleUnknown(displayOriginal, options);
        }

        var rule = match.Rule;
        var age = ResolveAge(rule, options.AmbiguousMappingPolicy, match.ExtractedAge);
        var normalizedLabel = age == 0 ? "ALL" : age + "+";
        var target = Render(age, options);
        if (string.IsNullOrEmpty(target))
        {
            return RatingConversionResult.NoMatch;
        }

        return new RatingConversionResult(true, displayOriginal, target, age, normalizedLabel, rule.Id, rule.System, rule.Name, false);
    }

    private RatingConversionResult HandleUnknown(string displayOriginal, RatingConversionOptions options)
    {
        return options.UnknownRatingBehavior switch
        {
            UnknownRatingBehavior.ConvertToUnrated => new RatingConversionResult(true, displayOriginal, "NR", null, null, null, null, null, true),
            UnknownRatingBehavior.HideRating => new RatingConversionResult(true, displayOriginal, string.Empty, null, null, null, null, null, true),
            _ => RatingConversionResult.NoMatch
        };
    }

    private RatingRuleMatch? FindRule(string displayOriginal, string normalizedOriginal, IReadOnlyList<RatingRule>? rules, bool custom)
    {
        return (rules ?? [])
            .Where(static rule => rule.Enabled)
            .Select(rule => MatchRule(rule, displayOriginal, normalizedOriginal))
            .Where(static result => result is not null)
            .Select(static result => result!)
            .OrderByDescending(result => result.Rule.Priority)
            .ThenByDescending(result => custom || result.Rule.IsCustom)
            .FirstOrDefault();
    }

    private static RatingRuleMatch? MatchRule(RatingRule rule, string displayOriginal, string normalizedOriginal)
    {
        if (rule.MatchType == RatingRuleMatchType.Regex)
        {
            if (string.IsNullOrWhiteSpace(rule.Pattern))
            {
                return null;
            }

            try
            {
                var match = Regex.Match(displayOriginal, rule.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexTimeout);
                if (!match.Success)
                {
                    return null;
                }

                int? extractedAge = null;
                if (rule.MinimumAgeFromGroup is int groupIndex && groupIndex > 0 && groupIndex < match.Groups.Count)
                {
                    if (int.TryParse(match.Groups[groupIndex].Value, out var age))
                    {
                        extractedAge = Math.Min(99, Math.Max(0, age));
                    }
                }

                return new RatingRuleMatch(rule, extractedAge);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (RegexMatchTimeoutException)
            {
                return null;
            }
        }

        foreach (var alias in rule.Aliases ?? [])
        {
            if (string.Equals(normalizedOriginal, NormalizeForComparison(alias), StringComparison.OrdinalIgnoreCase))
            {
                return new RatingRuleMatch(rule, null);
            }
        }

        return null;
    }

    private static int ResolveAge(RatingRule rule, AmbiguousMappingPolicy policy, int? extractedAge)
    {
        if (extractedAge is int ageFromGroup)
        {
            return Math.Min(99, Math.Max(0, ageFromGroup));
        }

        var age = policy switch
        {
            AmbiguousMappingPolicy.Conservative => rule.ConservativeAge ?? rule.MinimumAge,
            AmbiguousMappingPolicy.Lenient => rule.LenientAge ?? rule.MinimumAge,
            _ => rule.MinimumAge
        };

        return Math.Min(99, Math.Max(0, age));
    }

    private string Render(int age, RatingConversionOptions options)
    {
        var preset = FindPreset(options.PresetId, options.CustomPresets) ?? FindPreset(RatingCatalog.AgeBasedPresetId, null);
        if (preset is null)
        {
            return age == 0 ? "ALL" : age + "+";
        }

        var rule = (preset.Render ?? [])
            .Where(renderRule => age >= renderRule.Min && age <= renderRule.Max && !string.IsNullOrWhiteSpace(renderRule.Label))
            .OrderBy(renderRule => renderRule.Max - renderRule.Min)
            .FirstOrDefault();

        return rule?.Label?.Trim() ?? (age == 0 ? "ALL" : age + "+");
    }

    private RatingRenderPreset? FindPreset(string? presetId, IReadOnlyList<RatingRenderPreset>? customPresets)
    {
        if (string.IsNullOrWhiteSpace(presetId))
        {
            presetId = RatingCatalog.AgeBasedPresetId;
        }

        return (customPresets ?? [])
            .Concat(_builtInPresets)
            .FirstOrDefault(preset => string.Equals(preset.Id, presetId, StringComparison.OrdinalIgnoreCase));
    }

    public static string NormalizeDisplayValue(string? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? string.Empty : normalized;
    }

    public static string NormalizeForComparison(string? value)
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

    private sealed class RatingRuleMatch
    {
        public RatingRuleMatch(RatingRule rule, int? extractedAge)
        {
            Rule = rule;
            ExtractedAge = extractedAge;
        }

        public RatingRule Rule { get; }

        public int? ExtractedAge { get; }
    }
}
