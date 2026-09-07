using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Features.Occasions;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GiftBase.Tests;

public class OccasionServiceTests
{
    private const int OwnerUserId = 1;
    private const int OtherUserId = 2;

    private static readonly DateOnly DateOfBirth = new(1990, 5, 24);

    private readonly TestDbContextFactory _dbContextFactory;
    private readonly OccasionService _occasionService;

    public OccasionServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _occasionService = new OccasionService(_dbContextFactory);
    }

    private async Task<Person> AddPersonAsync(int userId, DateOnly? dateOfBirth = null, string firstName = "John")
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person(firstName, "Doe", dateOfBirth ?? DateOfBirth, Relation.Friend, userId);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        return person;
    }

    private async Task<Occasion> AddOccasionAsync(int personId, OccasionType type, string? title, DateOnly date, bool isRecurring)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var occasion = new Occasion(type, title, date, isRecurring, personId);
        dbContext.Occasions.Add(occasion);
        await dbContext.SaveChangesAsync();

        return occasion;
    }

    [Fact]
    public async Task GetOccasionsAsync_ShouldReturnOccasionsSortedByNextOccurrence()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var person = await AddPersonAsync(OwnerUserId);
        var inTwoMonths = await AddOccasionAsync(person.Id, OccasionType.Custom, "Später", today.AddMonths(2), false);
        var inOneMonth = await AddOccasionAsync(person.Id, OccasionType.Custom, "Früher", today.AddMonths(1), false);

        // Act
        var occasions = await _occasionService.GetOccasionsAsync(person.Id, OwnerUserId);

        // Assert
        occasions.Count.ShouldBe(2);
        occasions[0].Id.ShouldBe(inOneMonth.Id);
        occasions[1].Id.ShouldBe(inTwoMonths.Id);
    }

    [Fact]
    public async Task GetOccasionsAsync_ShouldReturnEmptyList_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddOccasionAsync(person.Id, OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true);

        // Act
        var occasions = await _occasionService.GetOccasionsAsync(person.Id, OtherUserId);

        // Assert
        occasions.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldAddChristmasOnDecember24th()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasionAddDto = new OccasionAddDto
        {
            Type = OccasionType.Christmas,
            Title = "Wird ignoriert",
            Date = new DateOnly(2026, 1, 1),
            IsRecurring = false,
            PersonId = person.Id
        };

        // Act
        var addedOccasion = await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId);

        // Assert
        addedOccasion.Id.ShouldBeGreaterThan(0);
        addedOccasion.Type.ShouldBe(OccasionType.Christmas);
        addedOccasion.Title.ShouldBeNull();
        addedOccasion.Date.Month.ShouldBe(12);
        addedOccasion.Date.Day.ShouldBe(24);
        addedOccasion.IsRecurring.ShouldBeTrue();
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldAddBirthdayFromPersonDateOfBirth()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasionAddDto = new OccasionAddDto
        {
            Type = OccasionType.Birthday,
            Date = new DateOnly(2026, 1, 1),
            IsRecurring = false,
            PersonId = person.Id
        };

        // Act
        var addedOccasion = await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId);

        // Assert
        addedOccasion.Type.ShouldBe(OccasionType.Birthday);
        addedOccasion.Date.ShouldBe(DateOfBirth);
        addedOccasion.IsRecurring.ShouldBeTrue();
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldAddCustomOccasion()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasionAddDto = new OccasionAddDto
        {
            Type = OccasionType.Custom,
            Title = "  Hochzeit  ",
            Date = new DateOnly(2027, 6, 5),
            IsRecurring = false,
            PersonId = person.Id
        };

        // Act
        var addedOccasion = await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId);

        // Assert
        addedOccasion.Type.ShouldBe(OccasionType.Custom);
        addedOccasion.Title.ShouldBe("Hochzeit");
        addedOccasion.Date.ShouldBe(new DateOnly(2027, 6, 5));
        addedOccasion.IsRecurring.ShouldBeFalse();
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldThrowConflictException_WhenBirthdayAlreadyExists()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddOccasionAsync(person.Id, OccasionType.Birthday, null, DateOfBirth, true);
        var occasionAddDto = new OccasionAddDto { Type = OccasionType.Birthday, PersonId = person.Id };

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId));

        exception.Message.ShouldBe("Geburtstag ist für diese Person bereits angelegt.");
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldThrowConflictException_WhenChristmasAlreadyExists()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddOccasionAsync(person.Id, OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true);
        var occasionAddDto = new OccasionAddDto { Type = OccasionType.Christmas, PersonId = person.Id };

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId));

        exception.Message.ShouldBe("Weihnachten ist für diese Person bereits angelegt.");
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldAllowMultipleCustomOccasions()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);
        var occasionAddDto = new OccasionAddDto
        {
            Type = OccasionType.Custom,
            Title = "Studienabschluss",
            Date = new DateOnly(2027, 9, 30),
            IsRecurring = false,
            PersonId = person.Id
        };

        // Act
        var addedOccasion = await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId);

        // Assert
        addedOccasion.Title.ShouldBe("Studienabschluss");
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldThrowConflictException_WhenPersonHasNoDateOfBirth()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", null, Relation.Friend, OwnerUserId);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();
        var occasionAddDto = new OccasionAddDto { Type = OccasionType.Birthday, PersonId = person.Id };

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId));

        exception.Message.ShouldBe("Für den Anlass Geburtstag muss bei der Person ein Geburtsdatum hinterlegt sein.");
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldThrowConflictException_WhenCustomOccasionHasNoTitle()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasionAddDto = new OccasionAddDto
        {
            Type = OccasionType.Custom,
            Title = "   ",
            Date = new DateOnly(2027, 6, 5),
            PersonId = person.Id
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId));

        exception.Message.ShouldBe("Für einen benutzerdefinierten Anlass ist ein Titel erforderlich.");
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldThrowNotFoundException_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasionAddDto = new OccasionAddDto { Type = OccasionType.Christmas, PersonId = person.Id };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _occasionService.AddOccasionAsync(occasionAddDto, OtherUserId));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task AddOccasionAsync_ShouldThrowNotFoundException_WhenPersonDoesNotExist()
    {
        // Arrange
        var occasionAddDto = new OccasionAddDto { Type = OccasionType.Christmas, PersonId = 999 };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _occasionService.AddOccasionAsync(occasionAddDto, OwnerUserId));

        exception.Message.ShouldBe("Person konnte nicht gefunden werden: 999");
    }
}
