using System.Collections.Generic;

namespace RatingStandardizer.Core;

public interface IRatingHistoryStore
{
    RatingHistoryEntry? Get(string itemId);

    IReadOnlyList<RatingHistoryEntry> GetAll();

    void Save(RatingHistoryEntry entry);

    void SaveMany(IEnumerable<RatingHistoryEntry> entries);
}
