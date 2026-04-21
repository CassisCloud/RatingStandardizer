using System.Collections.Generic;

namespace RatingStandardizer.Core;

/// <summary>
/// Exposes the configuration needed by the rating standardizer.
/// </summary>
public interface IRatingStandardizerConfiguration
{
    /// <summary>
    /// Gets a value indicating whether the plugin is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the configured rating mappings.
    /// </summary>
    IReadOnlyList<RatingMapping> Mappings { get; }
}
