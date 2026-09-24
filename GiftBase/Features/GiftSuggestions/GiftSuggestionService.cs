using System.Text.Json;
using System.Text.Json.Serialization;
using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using GiftBase.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.GiftSuggestions;

public class GiftSuggestionService(IDbContextFactory<GiftBaseDbContext> dbContextFactory, IGeminiClient geminiClient) : IGiftSuggestionService
{
    private const int MinimumSuggestionCount = 3;

    public async Task<List<GiftSuggestionDto>> GenerateAsync(GiftSuggestionGenerateDto giftSuggestionGenerateDto, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var person = await dbContext.Persons
            .Where(p => p.Id == giftSuggestionGenerateDto.PersonId && p.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {giftSuggestionGenerateDto.PersonId}");

        var occasion = await dbContext.Occasions
            .SingleOrDefaultAsync(o => o.Id == giftSuggestionGenerateDto.OccasionId && o.PersonId == person.Id)
                ?? throw new NotFoundException($"Anlass konnte nicht gefunden werden: {giftSuggestionGenerateDto.OccasionId}");

        var utcNow = DateTime.UtcNow;

        var quota = await dbContext.GiftSuggestionQuotas.SingleOrDefaultAsync(q => q.UserId == currentUserId);

        if (quota is null)
        {
            quota = new GiftSuggestionQuota(currentUserId);
            dbContext.GiftSuggestionQuotas.Add(quota);
        }

        if (quota.IsExhausted(utcNow))
        {
            throw new ConflictException(
                $"Tageslimit für KI-Vorschläge erreicht. Nächste Generierung möglich ab {quota.ResetsAt.ToLocalTime():dd.MM.yyyy HH:mm} Uhr.");
        }

        quota.RegisterRequest(utcNow);
        await dbContext.SaveChangesAsync();

        var existingGifts = await dbContext.Gifts
            .Where(g => g.PersonId == person.Id)
            .ToListAsync();

        var occasions = await dbContext.Occasions
            .Where(o => o.PersonId == person.Id)
            .ToListAsync();

        var input = GiftSuggestionPrompt.BuildInput(
            person,
            occasion,
            giftSuggestionGenerateDto.Budget,
            giftSuggestionGenerateDto.AdditionalHint,
            existingGifts,
            occasions,
            DateOnly.FromDateTime(utcNow));

        var json = await geminiClient.GenerateJsonAsync(
            GiftSuggestionPrompt.SystemInstruction, input, GiftSuggestionPrompt.ResponseSchema, CancellationToken.None);

        var suggestions = ParseSuggestions(json);

        var withinBudget = suggestions.Where(s => s.Price <= giftSuggestionGenerateDto.Budget).ToList();

        if (withinBudget.Count < MinimumSuggestionCount)
        {
            throw new ExternalServiceException(
                "Die KI konnte nicht genügend Vorschläge innerhalb des Budgets liefern. Bitte versuche es erneut.");
        }

        return withinBudget;
    }

    private static List<GiftSuggestionDto> ParseSuggestions(string json)
    {
        List<GeminiSuggestion>? rawSuggestions;

        try
        {
            rawSuggestions = JsonSerializer.Deserialize<List<GeminiSuggestion>>(json);
        }
        catch (JsonException)
        {
            throw new ExternalServiceException("Die Antwort der KI konnte nicht gelesen werden. Bitte versuche es erneut.");
        }

        if (rawSuggestions is null || rawSuggestions.Count == 0)
        {
            throw new ExternalServiceException("Die KI hat keine Vorschläge geliefert. Bitte versuche es erneut.");
        }

        return [.. rawSuggestions.Select(ToDto)];
    }

    private static GiftSuggestionDto ToDto(GeminiSuggestion suggestion)
    {
        if (suggestion.Preis < 0)
        {
            throw new ExternalServiceException("Die KI hat einen ungültigen Preis geliefert. Bitte versuche es erneut.");
        }

        return new GiftSuggestionDto
        {
            Type = ParseType(suggestion.Typ),
            Title = suggestion.Titel.NormalizeRequired(),
            Reason = suggestion.Begruendung.NormalizeRequired(),
            SearchTerm = suggestion.Suchbegriff.NormalizeRequired(),
            Price = suggestion.Preis
        };
    }

    private static GiftSuggestionType ParseType(string? typ) => typ switch
    {
        "Produkt" => GiftSuggestionType.Product,
        "Dienstleistung" => GiftSuggestionType.Service,
        "Geld" => GiftSuggestionType.Money,
        _ => throw new ExternalServiceException("Die KI hat einen unbekannten Vorschlagstyp geliefert. Bitte versuche es erneut.")
    };

    private sealed class GeminiSuggestion
    {
        [JsonPropertyName("typ")]
        public string? Typ { get; set; }
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = null!;
        [JsonPropertyName("begruendung")]
        public string Begruendung { get; set; } = null!;
        [JsonPropertyName("suchbegriff")]
        public string Suchbegriff { get; set; } = null!;
        [JsonPropertyName("preis")]
        public decimal Preis { get; set; }
    }
}
