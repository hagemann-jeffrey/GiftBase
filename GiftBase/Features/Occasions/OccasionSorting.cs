using GiftBase.Core.Entities;

namespace GiftBase.Features.Occasions;

public static class OccasionSorting
{
    public static List<Occasion> SortByNextOccurrence(this IEnumerable<Occasion> occasions, DateOnly today) =>
        [.. occasions
            .OrderBy(o => o.IsPast(today))
            .ThenBy(o => o.GetNextOccurrence(today))
            .ThenBy(o => o.Id)];
}
