using GiftBase.Core.Dtos.GiftSuggestions;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Features.GiftSuggestions;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace GiftBase.Tests.Features.GiftSuggestions;

public class GiftSuggestionServiceTests
{
    private const int OwnerUserId = 1;
    private const int OtherUserId = 2;

    private const string ValidResponseJson = """
        [
          { "typ": "Produkt", "titel": "Kopfhörer", "begruendung": "Passt zu Technik-Interesse", "suchbegriff": "Kopfhörer kabellos", "preis": 45 },
          { "typ": "Dienstleistung", "titel": "Kochkurs", "begruendung": "Gemeinsames Erlebnis", "suchbegriff": "Kochkurs Gutschein", "preis": 49.9 },
          { "typ": "Geld", "titel": "Geldgeschenk", "begruendung": "Flexibel einsetzbar", "suchbegriff": "Geldgeschenk", "preis": 30 },
          { "typ": "Produkt", "titel": "Buch", "begruendung": "Passt zu Lese-Interesse", "suchbegriff": "Roman Bestseller", "preis": 20 },
          { "typ": "Dienstleistung", "titel": "Konzertticket", "begruendung": "Gemeinsames Erlebnis", "suchbegriff": "Konzertticket", "preis": 48 }
        ]
        """;

    private readonly TestDbContextFactory _dbContextFactory;
    private readonly IGeminiClient _geminiClient;
    private readonly GiftSuggestionService _giftSuggestionService;

    public GiftSuggestionServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _geminiClient = Substitute.For<IGeminiClient>();
        _giftSuggestionService = new GiftSuggestionService(_dbContextFactory, _geminiClient);

        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidResponseJson);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowNotFoundException_WhenPersonDoesNotBelongToUser()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OtherUserId));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowNotFoundException_WhenOccasionDoesNotBelongToPerson()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var otherPerson = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(otherPerson.Id);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));

        exception.Message.ShouldBe($"Anlass konnte nicht gefunden werden: {occasion.Id}");
    }

    [Fact]
    public async Task GenerateAsync_ShouldAllowFiveRequestsWithinOneDay()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);

        // Act & Assert
        for (var i = 0; i < 5; i++)
        {
            await Should.NotThrowAsync(async () =>
                await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
        }
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowConflictException_OnSixthRequestWithinOneDay()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);

        for (var i = 0; i < 5; i++)
        {
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId);
        }

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldTrackQuotaPerUser_NotGlobally()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        var otherPerson = await AddPersonAsync(OtherUserId);
        var otherOccasion = await AddOccasionAsync(otherPerson.Id);

        for (var i = 0; i < 5; i++)
        {
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId);
        }

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(otherPerson.Id, otherOccasion.Id), OtherUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldCountAttempt_WhenClientThrows()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<string>(_ => throw new ExternalServiceException("Ausfall"));

        for (var i = 0; i < 5; i++)
        {
            await Should.ThrowAsync<ExternalServiceException>(async () =>
                await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
        }

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowExternalServiceException_WhenResponseIsInvalidJson()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("kein json");

        // Act & Assert
        await Should.ThrowAsync<ExternalServiceException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowExternalServiceException_WhenResponseIsEmptyArray()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("[]");

        // Act & Assert
        await Should.ThrowAsync<ExternalServiceException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowExternalServiceException_WhenTypeIsUnknown()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""[{ "typ": "Unbekannt", "titel": "X", "begruendung": "Y", "suchbegriff": "Z", "preis": 10 }]""");

        // Act & Assert
        await Should.ThrowAsync<ExternalServiceException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowExternalServiceException_WhenPriceIsNegative()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""[{ "typ": "Produkt", "titel": "X", "begruendung": "Y", "suchbegriff": "Z", "preis": -10 }]""");

        // Act & Assert
        await Should.ThrowAsync<ExternalServiceException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldFilterOutSuggestionsAboveBudget()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""
                [
                  { "typ": "Produkt", "titel": "Im Budget 1", "begruendung": "B", "suchbegriff": "S", "preis": 10 },
                  { "typ": "Produkt", "titel": "Im Budget 2", "begruendung": "B", "suchbegriff": "S", "preis": 20 },
                  { "typ": "Produkt", "titel": "Im Budget 3", "begruendung": "B", "suchbegriff": "S", "preis": 30 },
                  { "typ": "Produkt", "titel": "Über Budget", "begruendung": "B", "suchbegriff": "S", "preis": 999 }
                ]
                """);

        // Act
        var suggestions = await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id, budget: 50), OwnerUserId);

        // Assert
        suggestions.Count.ShouldBe(3);
        suggestions.ShouldNotContain(s => s.Title == "Über Budget");
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowExternalServiceException_WhenFewerThanThreeSuggestionsRemainAfterBudgetFilter()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);
        _geminiClient
            .GenerateJsonAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""
                [
                  { "typ": "Produkt", "titel": "Im Budget", "begruendung": "B", "suchbegriff": "S", "preis": 10 },
                  { "typ": "Produkt", "titel": "Über Budget 1", "begruendung": "B", "suchbegriff": "S", "preis": 999 },
                  { "typ": "Produkt", "titel": "Über Budget 2", "begruendung": "B", "suchbegriff": "S", "preis": 999 }
                ]
                """);

        // Act & Assert
        await Should.ThrowAsync<ExternalServiceException>(async () =>
            await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id, budget: 50), OwnerUserId));
    }

    [Fact]
    public async Task GenerateAsync_ShouldMapValidResponseToDtos()
    {
        // Arrange
        var person = await AddPersonAsync(OwnerUserId);
        var occasion = await AddOccasionAsync(person.Id);

        // Act
        var suggestions = await _giftSuggestionService.GenerateAsync(BuildRequest(person.Id, occasion.Id, budget: 50), OwnerUserId);

        // Assert
        suggestions.Count.ShouldBe(5);
        var headphones = suggestions.Single(s => s.Title == "Kopfhörer");
        headphones.Type.ShouldBe(GiftSuggestionType.Product);
        headphones.Reason.ShouldBe("Passt zu Technik-Interesse");
        headphones.SearchTerm.ShouldBe("Kopfhörer kabellos");
        headphones.Price.ShouldBe(45m);

        var money = suggestions.Single(s => s.Title == "Geldgeschenk");
        money.Type.ShouldBe(GiftSuggestionType.Money);

        var service = suggestions.Single(s => s.Title == "Kochkurs");
        service.Type.ShouldBe(GiftSuggestionType.Service);
    }

    private static GiftSuggestionGenerateDto BuildRequest(int personId, int occasionId, decimal budget = 100) => new()
    {
        PersonId = personId,
        OccasionId = occasionId,
        Budget = budget
    };

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

        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(2026, 5, 17), true, personId);
        dbContext.Occasions.Add(occasion);
        await dbContext.SaveChangesAsync();

        return occasion;
    }
}
