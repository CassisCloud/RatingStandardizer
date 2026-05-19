using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RatingStandardizer.Core;

public static class RatingConfigurationValidator
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    public static RatingValidationResult Validate(IReadOnlyList<RatingRule>? customRules, IReadOnlyList<RatingRenderPreset>? customPresets)
    {
        var result = new RatingValidationResult();
        ValidateRules(customRules ?? [], result);
        ValidatePresets(customPresets ?? [], result);
        return result;
    }

    private static void ValidateRules(IReadOnlyList<RatingRule> rules, RatingValidationResult result)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            var label = string.IsNullOrWhiteSpace(rule.Name) ? $"Rule {index + 1}" : rule.Name;

            ValidateAge(rule.MinimumAge, $"{label} minimum age", result);
            ValidateOptionalAge(rule.ConservativeAge, $"{label} conservative age", result);
            ValidateOptionalAge(rule.LenientAge, $"{label} lenient age", result);

            if (rule.MatchType == RatingRuleMatchType.Regex)
            {
                if (string.IsNullOrWhiteSpace(rule.Pattern))
                {
                    result.Errors.Add($"{label}: regex pattern is required.");
                }
                else
                {
                    try
                    {
                        _ = new Regex(rule.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexTimeout);
                    }
                    catch (ArgumentException ex)
                    {
                        result.Errors.Add($"{label}: regex pattern is invalid. {ex.Message}");
                    }
                }

                if (rule.MinimumAgeFromGroup is int group && group < 1)
                {
                    result.Errors.Add($"{label}: age group must be 1 or greater.");
                }
            }
            else
            {
                var ruleAliases = (rule.Aliases ?? []).Where(alias => !string.IsNullOrWhiteSpace(alias)).ToList();
                if (ruleAliases.Count == 0)
                {
                    result.Errors.Add($"{label}: at least one alias is required.");
                }

                foreach (var alias in ruleAliases)
                {
                    var normalized = RatingConverter.NormalizeForComparison(alias);
                    if (aliases.TryGetValue(normalized, out var existing))
                    {
                        result.Warnings.Add($"Alias '{alias}' is duplicated by '{existing}' and '{label}'. Higher priority wins.");
                    }
                    else
                    {
                        aliases[normalized] = label;
                    }
                }
            }
        }
    }

    private static void ValidatePresets(IReadOnlyList<RatingRenderPreset> presets, RatingValidationResult result)
    {
        foreach (var preset in presets)
        {
            var label = string.IsNullOrWhiteSpace(preset.Name) ? preset.Id : preset.Name;
            if (string.IsNullOrWhiteSpace(preset.Id))
            {
                result.Errors.Add("Custom preset id is required.");
            }

            var ranges = preset.Render ?? [];
            if (ranges.Count == 0)
            {
                result.Errors.Add($"{label}: at least one render range is required.");
                continue;
            }

            var coverage = new bool[100];
            foreach (var range in ranges)
            {
                ValidateAge(range.Min, $"{label} render min", result);
                ValidateAge(range.Max, $"{label} render max", result);
                if (range.Min > range.Max)
                {
                    result.Errors.Add($"{label}: render min must be less than or equal to max.");
                }

                if (string.IsNullOrWhiteSpace(range.Label))
                {
                    result.Errors.Add($"{label}: render label is required.");
                }

                for (var age = Math.Max(0, range.Min); age <= Math.Min(99, range.Max); age++)
                {
                    if (coverage[age])
                    {
                        result.Warnings.Add($"{label}: render ranges overlap at age {age}.");
                    }

                    coverage[age] = true;
                }
            }

            var uncovered = coverage.Select((covered, age) => new { covered, age }).Where(entry => !entry.covered).Select(entry => entry.age).ToList();
            if (uncovered.Count > 0)
            {
                result.Warnings.Add($"{label}: render ranges do not cover all ages 0-99.");
            }
        }
    }

    private static void ValidateAge(int age, string label, RatingValidationResult result)
    {
        if (age < 0 || age > 99)
        {
            result.Errors.Add($"{label} must be between 0 and 99.");
        }
    }

    private static void ValidateOptionalAge(int? age, string label, RatingValidationResult result)
    {
        if (age is int value)
        {
            ValidateAge(value, label, result);
        }
    }
}
