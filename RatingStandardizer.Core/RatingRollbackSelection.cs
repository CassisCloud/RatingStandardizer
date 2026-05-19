using System.Collections.Generic;

namespace RatingStandardizer.Core;

public sealed class RatingRollbackSelection
{
    public bool UseConfiguredTargetLibraries { get; set; } = true;

    public List<string> LibraryIds { get; set; } = [];

    public List<string> ItemIds { get; set; } = [];

    public List<string> UpdatedAtKeys { get; set; } = [];
}
