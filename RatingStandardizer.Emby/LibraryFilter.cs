using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;

namespace RatingStandardizer.Emby;

internal static class LibraryFilter
{
    public static HashSet<string>? CreateTargetLookup(IReadOnlyCollection<string>? targetLibraryIds)
    {
        if (targetLibraryIds is null || targetLibraryIds.Count == 0)
        {
            return null;
        }

        var lookup = targetLibraryIds
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id => NormalizeId(id))
            .Where(static id => id.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        return lookup.Count > 0 ? lookup : null;
    }

    public static bool IsMatch(BaseItem item, HashSet<string>? targetLookup)
    {
        if (targetLookup is null)
        {
            return true;
        }

        for (BaseItem? current = item; current is not null; current = current.GetParent())
        {
            if (ContainsId(current.Id, targetLookup))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsId(object? id, HashSet<string> targetLookup)
    {
        if (id is null)
        {
            return false;
        }

        var normalizedId = NormalizeId(id.ToString());
        return normalizedId.Length > 0 && targetLookup.Contains(normalizedId);
    }

    private static string NormalizeId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        return id.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }
}
