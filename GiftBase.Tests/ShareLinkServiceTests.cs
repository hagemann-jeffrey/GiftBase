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

    // GetSharedGiftListAsync

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldReturnOnlyIdeaGifts_WhenShareLinkIsScopedToThePerson()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddGiftAsync(person.Id, "Kaffeemaschine", GiftStatus.Idea);
        await AddGiftAsync(person.Id, "Buch", GiftStatus.Idea);
        await AddGiftAsync(person.Id, "Bereits gekauft", GiftStatus.Bought);
        await AddGiftAsync(person.Id, "Schon verschenkt", GiftStatus.Given);

        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act
        var sharedGiftList = await _shareLinkService.GetSharedGiftListAsync(shareLink.Token);

        // Assert
        sharedGiftList.Gifts.Count.ShouldBe(2);
        sharedGiftList.Gifts.Select(g => g.Title).ShouldBe(["Buch", "Kaffeemaschine"]);
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldExposeOnlyTheFirstName_WhenShareLinkIsValid()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act
        var sharedGiftList = await _shareLinkService.GetSharedGiftListAsync(shareLink.Token);

        // Assert
        sharedGiftList.RecipientFirstName.ShouldBe("Anna");
        sharedGiftList.OccasionTitle.ShouldBeNull();
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldReturnOnlyTheSharedFields_WhenGiftIsReturned()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        await AddGiftAsync(person.Id, "Kaffeemaschine", GiftStatus.Idea);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act
        var sharedGiftList = await _shareLinkService.GetSharedGiftListAsync(shareLink.Token);

        // Assert
        var sharedGift = sharedGiftList.Gifts.Single();
        sharedGift.Title.ShouldBe("Kaffeemaschine");
        sharedGift.Note.ShouldBe("Notiz");
        sharedGift.Price.ShouldBe(42.50m);
        sharedGift.Link.ShouldBe("https://example.com/artikel");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldReturnOnlyGiftsOfThatOccasion_WhenShareLinkIsScopedToAnOccasion()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        await AddGiftAsync(person.Id, "Weihnachtsgeschenk", GiftStatus.Idea, occasion.Id);
        await AddGiftAsync(person.Id, "Ohne Anlass", GiftStatus.Idea);

        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id, OccasionId = occasion.Id }, OwnerUserId);

        // Act
        var sharedGiftList = await _shareLinkService.GetSharedGiftListAsync(shareLink.Token);

        // Assert
        sharedGiftList.Gifts.Single().Title.ShouldBe("Weihnachtsgeschenk");
        sharedGiftList.OccasionTitle.ShouldBe("Weihnachten");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldReturnEmptyGiftList_WhenPersonHasNoIdeas()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        // Act
        var sharedGiftList = await _shareLinkService.GetSharedGiftListAsync(shareLink.Token);

        // Assert
        sharedGiftList.Gifts.ShouldBeEmpty();
        sharedGiftList.RecipientFirstName.ShouldBe("Anna");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldThrowNotFoundException_WhenShareLinkIsExpired()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await AddExpiredShareLinkAsync(OwnerUserId, person.Id, null);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.GetSharedGiftListAsync(shareLink.Token));

        exception.Message.ShouldBe("Link konnte nicht gefunden werden.");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldThrowNotFoundException_WhenShareLinkWasDeleted()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);
        await _shareLinkService.DeleteShareLinkAsync(shareLink.Id, OwnerUserId);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.GetSharedGiftListAsync(shareLink.Token));
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldThrowNotFoundException_WhenThePersonWasDeleted()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        await using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            dbContext.Persons.Remove(await dbContext.Persons.SingleAsync(p => p.Id == person.Id));
            await dbContext.SaveChangesAsync();
        }

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.GetSharedGiftListAsync(shareLink.Token));

        exception.Message.ShouldBe("Link konnte nicht gefunden werden.");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldThrowNotFoundException_WhenThePersonNowBelongsToAnotherUser()
    {
        // Arrange — die verwaiste PersonId zeigt auf eine Person eines anderen Nutzers
        var person = await AddPersonAsync(OwnerUserId);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id }, OwnerUserId);

        await using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            var linkInDb = await dbContext.ShareLinks.SingleAsync(s => s.Id == shareLink.Id);
            dbContext.ShareLinks.Remove(linkInDb);
            dbContext.ShareLinks.Add(new ShareLink(shareLink.Token, OtherUserId, person.Id, null, DateTime.UtcNow));
            await dbContext.SaveChangesAsync();
        }

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.GetSharedGiftListAsync(shareLink.Token));

        exception.Message.ShouldBe("Link konnte nicht gefunden werden.");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldThrowNotFoundException_WhenTheOccasionWasDeleted()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        var shareLink = await _shareLinkService.AddShareLinkAsync(
            new ShareLinkAddDto { PersonId = person.Id, OccasionId = occasion.Id }, OwnerUserId);

        await using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            dbContext.Occasions.Remove(await dbContext.Occasions.SingleAsync(o => o.Id == occasion.Id));
            await dbContext.SaveChangesAsync();
        }

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.GetSharedGiftListAsync(shareLink.Token));

        exception.Message.ShouldBe("Link konnte nicht gefunden werden.");
    }

    [Fact]
    public async Task GetSharedGiftListAsync_ShouldThrowNotFoundExceptionWithoutEchoingTheToken_WhenTokenIsUnknown()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _shareLinkService.GetSharedGiftListAsync("unbekannter-token"));

        exception.Message.ShouldBe("Link konnte nicht gefunden werden.");
        exception.Message.ShouldNotContain("unbekannter-token");
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
