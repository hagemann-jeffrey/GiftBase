using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Features.Persons;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests;

public class PersonSortingTests
{
    private static readonly DateOnly Today = new(2026, 5, 12);

    private readonly TestDbContextFactory _dbContextFactory = new();

    private async Task<Person> AddPersonAsync(string firstName, string lastName = "Doe")
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person(firstName, lastName, null, Relation.Friend, 1);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        return person;
    }

    [Fact]
    public async Task SortByNextOccasion_ShouldOrderByNextOccurrenceDate()
    {
        // Arrange
        var soon = await AddPersonAsync("Anna");
        var later = await AddPersonAsync("Bea");
        var nextOccasions = new Dictionary<int, Occasion>
        {
            [soon.Id] = new Occasion(OccasionType.Custom, "Bald", Today.AddDays(5), false, soon.Id),
            [later.Id] = new Occasion(OccasionType.Custom, "Später", Today.AddMonths(2), false, later.Id)
        };

        // Act
        var sorted = new[] { later, soon }.SortByNextOccasion(nextOccasions, Today);

        // Assert
        sorted[0].ShouldBe(soon);
        sorted[1].ShouldBe(later);
    }

    [Fact]
    public async Task SortByNextOccasion_ShouldPutPersonsWithoutOccasionLast_AndOrderThemAlphabetically()
    {
        // Arrange
        var withOccasion = await AddPersonAsync("Carl");
        var zoe = await AddPersonAsync("Zoe");
        var anna = await AddPersonAsync("Anna");
        var nextOccasions = new Dictionary<int, Occasion>
        {
            [withOccasion.Id] = new Occasion(OccasionType.Custom, "Bald", Today.AddDays(5), false, withOccasion.Id)
        };

        // Act
        var sorted = new[] { zoe, withOccasion, anna }.SortByNextOccasion(nextOccasions, Today);

        // Assert
        sorted[0].ShouldBe(withOccasion);
        sorted[1].ShouldBe(anna);
        sorted[2].ShouldBe(zoe);
    }

    [Fact]
    public void SortByNextOccasion_ShouldReturnEmptyList_WhenThereAreNoPersons()
    {
        // Act
        var sorted = Array.Empty<Person>().SortByNextOccasion(new Dictionary<int, Occasion>(), Today);

        // Assert
        sorted.ShouldBeEmpty();
    }
}
