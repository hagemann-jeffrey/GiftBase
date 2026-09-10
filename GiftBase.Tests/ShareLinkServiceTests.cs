using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Features.Sharing;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GiftBase.Tests;

public class ShareLinkServiceTests
{
    private const int OwnerUserId = 1;
    private const int OtherUserId = 2;

    private readonly TestDbContextFactory _dbContextFactory;
    private readonly ShareLinkService _shareLinkService;

    public ShareLinkServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _shareLinkService = new ShareLinkService(_dbContextFactory);
    }

    // AddShareLinkAsync

    [Fact]
    public async Task AddShareLinkAsync_ShouldCreateLinkWithTokenAndOwner_WhenPersonBelongsToUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);

        // Act
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Assert
        shareLink.Token.ShouldNotBeNullOrWhiteSpace();
        shareLink.Token.Length.ShouldBeGreaterThanOrEqualTo(32);
        shareLink.UserId.ShouldBe(OwnerUserId);
        shareLink.PersonId.ShouldBe(person.Id);
        shareLink.OccasionId.ShouldBeNull();
        shareLink.IsExpired(DateTime.UtcNow).ShouldBeFalse();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var shareLinkInDb = await dbContext.ShareLinks.SingleOrDefaultAsync(s => s.Id == shareLink.Id);
        shareLinkInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddShareLinkAsync_ShouldGenerateDifferentTokens_WhenCalledForDifferentScopes()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);

        // Act
        var personLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);
        var occasionLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id, OccasionId = occasion.Id }, OwnerUserId);

        // Assert
        occasionLink.Token.ShouldNotBe(personLink.Token);
        occasionLink.OccasionId.ShouldBe(occasion.Id);
    }

    [Fact]
    public async Task AddShareLinkAsync_ShouldThrowNotFoundException_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.AddShareLinkAsync(new ShareLinkAddDto { PersonId = person.Id }, OtherUserId));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task AddShareLinkAsync_ShouldThrowNotFoundException_WhenOccasionBelongsToAnotherPerson()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var otherPerson = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(otherPerson.Id);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.AddShareLinkAsync(
                new ShareLinkAddDto { PersonId = person.Id, OccasionId = occasion.Id }, OwnerUserId));

        exception.Message.ShouldBe($"Anlass konnte nicht gefunden werden: {occasion.Id}");
    }

    [Fact]
    public async Task AddShareLinkAsync_ShouldThrowConflictException_WhenValidLinkForSameScopeAlreadyExists()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await _shareLinkService.AddShareLinkAsync(new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _shareLinkService.AddShareLinkAsync(new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId));

        exception.Message.ShouldBe("Für diesen Umfang existiert bereits ein gültiger Link.");
    }

    [Fact]
    public async Task AddShareLinkAsync_ShouldCreateNewLink_WhenExistingLinkForSameScopeIsExpired()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddExpiredShareLinkAsync(OwnerUserId, person.Id, null);

        // Act
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Assert
        shareLink.IsExpired(DateTime.UtcNow).ShouldBeFalse();
    }

    // GetShareLinksAsync

    [Fact]
    public async Task GetShareLinksAsync_ShouldReturnLinksOfThePerson_WhenPersonBelongsToUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        await _shareLinkService.AddShareLinkAsync(new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);
        await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id, OccasionId = occasion.Id }, OwnerUserId);

        // Act
        var shareLinks = await _shareLinkService.GetShareLinksAsync(person.Id, OwnerUserId);

        // Assert
        shareLinks.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetShareLinksAsync_ShouldAlsoReturnExpiredLinks_SoTheUserCanDeleteThem()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddExpiredShareLinkAsync(OwnerUserId, person.Id, null);

        // Act
        var shareLinks = await _shareLinkService.GetShareLinksAsync(person.Id, OwnerUserId);

        // Assert
        shareLinks.Single().IsExpired(DateTime.UtcNow).ShouldBeTrue();
    }

    [Fact]
    public async Task GetShareLinksAsync_ShouldReturnEmptyList_WhenPersonBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await _shareLinkService.AddShareLinkAsync(new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act
        var shareLinks = await _shareLinkService.GetShareLinksAsync(person.Id, OtherUserId);

        // Assert
        shareLinks.ShouldBeEmpty();
    }

    // DeleteShareLinkAsync

    [Fact]
    public async Task DeleteShareLinkAsync_ShouldRemoveLink_WhenLinkBelongsToUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act
        await _shareLinkService.DeleteShareLinkAsync(shareLink.Id, OwnerUserId);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var shareLinkInDb = await dbContext.ShareLinks.SingleOrDefaultAsync(s => s.Id == shareLink.Id);
        shareLinkInDb.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteShareLinkAsync_ShouldThrowNotFoundException_WhenLinkBelongsToAnotherUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.DeleteShareLinkAsync(shareLink.Id, OtherUserId));

        exception.Message.ShouldBe($"Link konnte nicht gefunden werden: {shareLink.Id}");
    }

    [Fact]
    public async Task DeleteShareLinkAsync_ShouldThrowNotFoundException_WhenLinkDoesNotExist()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.DeleteShareLinkAsync(999, OwnerUserId));

        exception.Message.ShouldBe("Link konnte nicht gefunden werden: 999");
    }

    // Arrange helpers

    private async Task<Person> AddPersonAsync(int userId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var person = new Person("Anna", "Beispiel", new DateOnly(1990, 5, 17), Relation.Family, userId);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        return person;
    }

    private async Task<Occasion> AddOccasionAsync(int personId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var occasion = new Occasion(OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true, personId);
        dbContext.Occasions.Add(occasion);
        await dbContext.SaveChangesAsync();

        return occasion;
    }

    private async Task<ShareLink> AddExpiredShareLinkAsync(int userId, int personId, int? occasionId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var shareLink = new ShareLink(
            $"expired-{Guid.NewGuid():N}",
            userId,
            personId,
            occasionId,
            DateTime.UtcNow - ShareLink.Lifetime - TimeSpan.FromDays(1));

        dbContext.ShareLinks.Add(shareLink);
        await dbContext.SaveChangesAsync();

        return shareLink;
    }

    private async Task<Gift> AddGiftAsync(int personId, string title, GiftStatus status, int? occasionId = null)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var gift = new Gift(title, "Notiz", "https://example.com/artikel", 42.50m, personId, occasionId);

        if (status != GiftStatus.Idea)
        {
            gift.Update(new GiftUpdateDto
            {
                Title = title,
                Note = "Notiz",
                Link = "https://example.com/artikel",
                Price = 42.50m,
                Status = status,
                OccasionId = occasionId
            });
        }

        dbContext.Gifts.Add(gift);
        await dbContext.SaveChangesAsync();

        return gift;
    }
}
