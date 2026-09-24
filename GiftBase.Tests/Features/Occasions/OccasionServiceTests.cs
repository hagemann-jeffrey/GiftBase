using GiftBase.Core.Dtos.Occasions;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Features.Occasions;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GiftBase.Tests.Features.Occasions;

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

    private async Task<Gift> AddGiftAsync(int personId, int? occasionId, string title = "Kaffeemaschine")
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var gift = new Gift(title, null, null, null, personId, occasionId);
        dbContext.Gifts.Add(gift);
        await dbContext.SaveChangesAsync();

        return gift;
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
    public async Task GetNextOccasionsByPersonAsync_ShouldReturnNearestUpcomingOccasionPerPerson()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var person = await AddPersonAsync(OwnerUserId);
        var soon = await AddOccasionAsync(person.Id, OccasionType.Custom, "Bald", today.AddDays(5), false);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Später", today.AddMonths(2), false);

        // Act
        var nextOccasions = await _occasionService.GetNextOccasionsByPersonAsync(OwnerUserId);

        // Assert
        nextOccasions.Count.ShouldBe(1);
        nextOccasions[person.Id].Id.ShouldBe(soon.Id);
    }

    [Fact]
    public async Task GetNextOccasionsByPersonAsync_ShouldSkipPerson_WhenOnlyOccasionIsPast()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var person = await AddPersonAsync(OwnerUserId);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Vorbei", today.AddDays(-5), false);

        // Act
        var nextOccasions = await _occasionService.GetNextOccasionsByPersonAsync(OwnerUserId);

        // Assert
        nextOccasions.ShouldNotContainKey(person.Id);
    }

    [Fact]
    public async Task GetNextOccasionsByPersonAsync_ShouldNotIncludeOccasions_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OtherUserId);
        await AddOccasionAsync(person.Id, OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true);

        // Act
        var nextOccasions = await _occasionService.GetNextOccasionsByPersonAsync(OwnerUserId);

        // Assert
        nextOccasions.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetNextOccasionsByPersonAsync_ShouldReturnEmptyDictionary_WhenUserHasNoOccasions()
    {
        // Arrange
        await AddPersonAsync(OwnerUserId);

        // Act
        var nextOccasions = await _occasionService.GetNextOccasionsByPersonAsync(OwnerUserId);

        // Assert
        nextOccasions.ShouldBeEmpty();
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

    [Fact]
    public async Task UpdateOccasionAsync_ShouldUpdateCustomOccasion()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);
        var occasionUpdateDto = new OccasionUpdateDto
        {
            Title = "Studienabschluss",
            Date = new DateOnly(2027, 9, 30),
            IsRecurring = true
        };

        // Act
        var updatedOccasion = await _occasionService.UpdateOccasionAsync(occasion.Id, OwnerUserId, occasionUpdateDto);

        // Assert
        updatedOccasion.Title.ShouldBe("Studienabschluss");
        updatedOccasion.Date.ShouldBe(new DateOnly(2027, 9, 30));
        updatedOccasion.IsRecurring.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateOccasionAsync_ShouldKeepGiftAssignment()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);
        var gift = await AddGiftAsync(person.Id, occasion.Id);

        // Act
        await _occasionService.UpdateOccasionAsync(occasion.Id, OwnerUserId, new OccasionUpdateDto
        {
            Title = "Studienabschluss",
            Date = new DateOnly(2027, 9, 30),
            IsRecurring = true
        });

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var giftInDb = await dbContext.Gifts.SingleAsync(g => g.Id == gift.Id);
        giftInDb.OccasionId.ShouldBe(occasion.Id);
        giftInDb.OccasionLabel.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateOccasionAsync_ShouldThrowConflictException_WhenOccasionTypeIsFixed()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true);

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _occasionService.UpdateOccasionAsync(occasion.Id, OwnerUserId, new OccasionUpdateDto
            {
                Title = "Heiligabend",
                Date = new DateOnly(2026, 12, 25),
                IsRecurring = false
            }));

        exception.Message.ShouldBe("Feste Anlässe können nicht bearbeitet werden.");
    }

    [Fact]
    public async Task UpdateOccasionAsync_ShouldThrowConflictException_WhenTitleIsMissing()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _occasionService.UpdateOccasionAsync(occasion.Id, OwnerUserId, new OccasionUpdateDto
            {
                Title = "   ",
                Date = new DateOnly(2027, 6, 5),
                IsRecurring = false
            }));

        exception.Message.ShouldBe("Für einen benutzerdefinierten Anlass ist ein Titel erforderlich.");
    }

    [Fact]
    public async Task UpdateOccasionAsync_ShouldThrowNotFoundException_WhenOccasionBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _occasionService.UpdateOccasionAsync(occasion.Id, OtherUserId, new OccasionUpdateDto
            {
                Title = "Studienabschluss",
                Date = new DateOnly(2027, 9, 30),
                IsRecurring = false
            }));

        exception.Message.ShouldBe($"Anlass konnte nicht gefunden werden: {occasion.Id}");
    }

    [Fact]
    public async Task UpdateOccasionAsync_ShouldThrowNotFoundException_WhenOccasionDoesNotExist()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _occasionService.UpdateOccasionAsync(999, OwnerUserId, new OccasionUpdateDto
            {
                Title = "Studienabschluss",
                Date = new DateOnly(2027, 9, 30),
                IsRecurring = false
            }));

        exception.Message.ShouldBe("Anlass konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task DeleteOccasionAsync_ShouldDeleteOccasion()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);

        // Act
        await _occasionService.DeleteOccasionAsync(occasion.Id, OwnerUserId);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var deletedOccasion = await dbContext.Occasions.SingleOrDefaultAsync(o => o.Id == occasion.Id);
        deletedOccasion.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteOccasionAsync_ShouldKeepGiftAndStoreOccasionSnapshot()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);
        var gift = await AddGiftAsync(person.Id, occasion.Id);

        // Act
        await _occasionService.DeleteOccasionAsync(occasion.Id, OwnerUserId);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var giftInDb = await dbContext.Gifts.SingleOrDefaultAsync(g => g.Id == gift.Id);
        giftInDb.ShouldNotBeNull();
        giftInDb.OccasionId.ShouldBeNull();
        giftInDb.OccasionLabel.ShouldBe("Hochzeit");
        giftInDb.OccasionYear.ShouldBe(2027);
    }

    [Fact]
    public async Task DeleteOccasionAsync_ShouldStoreTypeNameAsSnapshotLabel_ForFixedOccasions()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Birthday, null, DateOfBirth, true);
        var gift = await AddGiftAsync(person.Id, occasion.Id);
        var expectedYear = occasion.GetNextOccurrence(DateOnly.FromDateTime(DateTime.Today)).Year;

        // Act
        await _occasionService.DeleteOccasionAsync(occasion.Id, OwnerUserId);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var giftInDb = await dbContext.Gifts.SingleAsync(g => g.Id == gift.Id);
        giftInDb.OccasionLabel.ShouldBe("Geburtstag");
        giftInDb.OccasionYear.ShouldBe(expectedYear);
    }

    [Fact]
    public async Task DeleteOccasionAsync_ShouldThrowNotFoundException_WhenOccasionBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _occasionService.DeleteOccasionAsync(occasion.Id, OtherUserId));

        exception.Message.ShouldBe($"Anlass konnte nicht gefunden werden: {occasion.Id}");
    }

    [Fact]
    public async Task DeleteOccasionAsync_ShouldThrowNotFoundException_WhenOccasionDoesNotExist()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _occasionService.DeleteOccasionAsync(999, OwnerUserId));

        exception.Message.ShouldBe("Anlass konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task DeleteOccasionAsync_ShouldStoreSnapshotForEveryAttachedGift()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), false);
        var firstGift = await AddGiftAsync(person.Id, occasion.Id, "Kaffeemaschine");
        var secondGift = await AddGiftAsync(person.Id, occasion.Id, "Buch");
        var unrelatedGift = await AddGiftAsync(person.Id, null, "Gutschein");

        // Act
        await _occasionService.DeleteOccasionAsync(occasion.Id, OwnerUserId);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var giftsInDb = await dbContext.Gifts.ToListAsync();
        giftsInDb.Count.ShouldBe(3);

        var firstGiftInDb = giftsInDb.Single(g => g.Id == firstGift.Id);
        firstGiftInDb.OccasionId.ShouldBeNull();
        firstGiftInDb.OccasionLabel.ShouldBe("Hochzeit");
        firstGiftInDb.OccasionYear.ShouldBe(2027);

        var secondGiftInDb = giftsInDb.Single(g => g.Id == secondGift.Id);
        secondGiftInDb.OccasionId.ShouldBeNull();
        secondGiftInDb.OccasionLabel.ShouldBe("Hochzeit");
        secondGiftInDb.OccasionYear.ShouldBe(2027);

        var unrelatedGiftInDb = giftsInDb.Single(g => g.Id == unrelatedGift.Id);
        unrelatedGiftInDb.OccasionId.ShouldBeNull();
        unrelatedGiftInDb.OccasionLabel.ShouldBeNull();
        unrelatedGiftInDb.OccasionYear.ShouldBeNull();
    }
}
