using System.Collections.Generic;

namespace RatingStandardizer.Core;

public class RatingRenderPreset
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<RatingRenderRule> Render { get; set; } = [];

    public bool IsCustom { get; set; }
}

public class RatingRenderRule
{
    public int Min { get; set; }

    public int Max { get; set; }

    public string Label { get; set; } = string.Empty;
}
