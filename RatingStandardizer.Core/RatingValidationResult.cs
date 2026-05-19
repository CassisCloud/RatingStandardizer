using System.Collections.Generic;

namespace RatingStandardizer.Core;

public sealed class RatingValidationResult
{
    public List<string> Errors { get; } = [];

    public List<string> Warnings { get; } = [];

    public bool IsValid => Errors.Count == 0;
}
