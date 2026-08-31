using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Features.Gifts;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GiftBase.Tests;

public class GiftServiceTests
{
    private const int OwnerUserId = 1;
    private const int OtherUserId = 2;

    private readonly TestDbContextFactory _dbContextFactory;
    private readonly GiftService _giftService;

    public GiftServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _giftService = new GiftService(_dbContextFactory);
    }

    private async Task<Person> AddPersonAsync(int userId, string firstName = "John")
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person(firstName, "Doe", DateOnly.Parse("1990-01-01"), Relation.Friend, userId);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        return person;
    }

    private async Task<Gift> AddGiftAsync(int personId, string title = "Kaffeemaschine")
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var gift = new Gift(title, null, null, null, personId);
        dbContext.Gifts.Add(gift);
        await dbContext.SaveChangesAsync();

        return gift;
    }

    [Fact]
    public async Task GetGiftsAsync_ShouldReturnGiftsForPerson()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddGiftAsync(person.Id, "Kaffeemaschine");
        await AddGiftAsync(person.Id, "Buch");

        // Act
        var gifts = await _giftService.GetGiftsAsync(person.Id, OwnerUserId);

        // Assert
        gifts.Count.ShouldBe(2);
        gifts.All(g => g.PersonId == person.Id).ShouldBeTrue();
    }

    [Fact]
    public async Task GetGiftsAsync_ShouldReturnEmptyList_WhenPersonHasNoGifts()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);

        // Act
        var gifts = await _giftService.GetGiftsAsync(person.Id, OwnerUserId);

        // Assert
        gifts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetGiftsAsync_ShouldReturnEmptyList_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddGiftAsync(person.Id);

        // Act
        var gifts = await _giftService.GetGiftsAsync(person.Id, OtherUserId);

        // Assert
        gifts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AddGiftAsync_ShouldAddGift()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var giftAddDto = new GiftAddDto
        {
            Title = "Kaffeemaschine",
            Note = "Am liebsten in Schwarz",
            Link = "https://example.com/kaffee",
            Price = 129.99m,
            Status = GiftStatus.Idea,
            PersonId = person.Id
        };

        // Act
        var addedGift = await _giftService.AddGiftAsync(giftAddDto, OwnerUserId);

        // Assert
        addedGift.Id.ShouldBeGreaterThan(0);
        addedGift.Title.ShouldBe("Kaffeemaschine");
        addedGift.Note.ShouldBe("Am liebsten in Schwarz");
        addedGift.Link.ShouldBe("https://example.com/kaffee");
        addedGift.Price.ShouldBe(129.99m);
        addedGift.Status.ShouldBe(GiftStatus.Idea);
        addedGift.PersonId.ShouldBe(person.Id);

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var giftInDb = await dbContext.Gifts.SingleOrDefaultAsync(g => g.Id == addedGift.Id);
        giftInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddGiftAsync_ShouldThrowNotFoundException_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var giftAddDto = new GiftAddDto
        {
            Title = "Kaffeemaschine",
            Status = GiftStatus.Idea,
            PersonId = person.Id
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftService.AddGiftAsync(giftAddDto, OtherUserId));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task AddGiftAsync_ShouldThrowNotFoundException_WhenPersonDoesNotExist()
    {
        // Arrange
        var giftAddDto = new GiftAddDto
        {
            Title = "Kaffeemaschine",
            Status = GiftStatus.Idea,
            PersonId = 999
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftService.AddGiftAsync(giftAddDto, OwnerUserId));

        exception.Message.ShouldBe("Person konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task UpdateGiftAsync_ShouldUpdateGift()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var gift = await AddGiftAsync(person.Id);
        var giftUpdateDto = new GiftUpdateDto
        {
            Title = "Espressomaschine",
            Note = "Doch lieber Espresso",
            Link = "https://example.com/espresso",
            Price = 249.50m,
            Status = GiftStatus.Bought
        };

        // Act
        var updatedGift = await _giftService.UpdateGiftAsync(gift.Id, OwnerUserId, giftUpdateDto);

        // Assert
        updatedGift.Title.ShouldBe("Espressomaschine");
        updatedGift.Note.ShouldBe("Doch lieber Espresso");
        updatedGift.Link.ShouldBe("https://example.com/espresso");
        updatedGift.Price.ShouldBe(249.50m);
        updatedGift.Status.ShouldBe(GiftStatus.Bought);
    }

    [Fact]
    public async Task UpdateGiftAsync_ShouldThrowNotFoundException_WhenGiftBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var gift = await AddGiftAsync(person.Id);
        var giftUpdateDto = new GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Bought
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftService.UpdateGiftAsync(gift.Id, OtherUserId, giftUpdateDto));

        exception.Message.ShouldBe($"Geschenkidee konnte nicht gefunden werden: {gift.Id}");
    }

    [Fact]
    public async Task UpdateGiftAsync_ShouldThrowNotFoundException_WhenGiftDoesNotExist()
    {
        // Arrange
        var giftUpdateDto = new GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Bought
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftService.UpdateGiftAsync(999, OwnerUserId, giftUpdateDto));

        exception.Message.ShouldBe("Geschenkidee konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task DeleteGiftAsync_ShouldDeleteGift()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var gift = await AddGiftAsync(person.Id);

        // Act
        await _giftService.DeleteGiftAsync(gift.Id, OwnerUserId);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var deletedGift = await dbContext.Gifts.SingleOrDefaultAsync(g => g.Id == gift.Id);
        deletedGift.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteGiftAsync_ShouldThrowNotFoundException_WhenGiftBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var gift = await AddGiftAsync(person.Id);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftService.DeleteGiftAsync(gift.Id, OtherUserId));

        exception.Message.ShouldBe($"Geschenkidee konnte nicht gefunden werden: {gift.Id}");

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var giftInDb = await dbContext.Gifts.SingleOrDefaultAsync(g => g.Id == gift.Id);
        giftInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteGiftAsync_ShouldThrowNotFoundException_WhenGiftDoesNotExist()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftService.DeleteGiftAsync(999, OwnerUserId));

        exception.Message.ShouldBe("Geschenkidee konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task GetGiftCountsByPersonAsync_ShouldReturnCountPerPerson()
    {
        // Arrange
        var personWithGifts = await AddPersonAsync(OwnerUserId, "John");
        var personWithoutGifts = await AddPersonAsync(OwnerUserId, "Jane");
        await AddGiftAsync(personWithGifts.Id, "Kaffeemaschine");
        await AddGiftAsync(personWithGifts.Id, "Buch");

        // Act
        var giftCounts = await _giftService.GetGiftCountsByPersonAsync(OwnerUserId);

        // Assert
        giftCounts[personWithGifts.Id].ShouldBe(2);
        giftCounts[personWithoutGifts.Id].ShouldBe(0);
    }

    [Fact]
    public async Task GetGiftCountsByPersonAsync_ShouldNotContainPersonsOfAnotherUser()
    {
        // Arrange
        var ownPerson = await AddPersonAsync(OwnerUserId, "John");
        var foreignPerson = await AddPersonAsync(OtherUserId, "Jane");
        await AddGiftAsync(foreignPerson.Id);

        // Act
        var giftCounts = await _giftService.GetGiftCountsByPersonAsync(OwnerUserId);

        // Assert
        giftCounts.Count.ShouldBe(1);
        giftCounts.ShouldContainKey(ownPerson.Id);
        giftCounts.ShouldNotContainKey(foreignPerson.Id);
    }

    [Fact]
    public async Task GetGiftCountsByPersonAsync_ShouldReturnEmptyDictionary_WhenUserHasNoPersons()
    {
        // Act
        var giftCounts = await _giftService.GetGiftCountsByPersonAsync(OwnerUserId);

        // Assert
        giftCounts.Count.ShouldBe(0);
    }
}