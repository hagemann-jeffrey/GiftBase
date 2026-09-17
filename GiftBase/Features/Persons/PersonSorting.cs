using GiftBase.Core.Entities;

namespace GiftBase.Features.Persons;

public static class PersonSorting
{
    public static List<Person> SortByNextOccasion(
        this IEnumerable<Person> persons,
        IReadOnlyDictionary<int, Occasion> nextOccasionsByPerson,
        DateOnly today) =>
        [.. persons
            .OrderBy(p => nextOccasionsByPerson.TryGetValue(p.Id, out var occasion)
                ? occasion.GetNextOccurrence(today)
                : DateOnly.MaxValue)
            .ThenBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.Id)];
}
