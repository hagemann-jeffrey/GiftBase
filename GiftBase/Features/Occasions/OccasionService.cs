using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.EntityFrameworkCore;
using Translations = GiftBase.Shared.Translations.Translations;

namespace GiftBase.Features.Occasions;

public class OccasionService(IDbContextFactory<GiftBaseDbContext> dbContextFactory) : IOccasionService
{
    public async Task<List<Occasion>> GetOccasionsAsync(int personId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var occasions = await dbContext.Occasions
            .Where(o => o.PersonId == personId && o.Person.UserId == currentUserId)
            .ToListAsync();

        return occasions.SortByNextOccurrence(DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<Dictionary<int, Occasion>> GetNextOccasionsByPersonAsync(int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var occasions = await dbContext.Occasions
            .Where(o => o.Person.UserId == currentUserId)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);

        return occasions
            .Where(o => !o.IsPast(today))
            .GroupBy(o => o.PersonId)
            .ToDictionary(g => g.Key, g => g.SortByNextOccurrence(today).First());
    }

    public async Task<Occasion> AddOccasionAsync(OccasionAddDto occasionAddDto, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var person = await dbContext.Persons
            .Where(p => p.Id == occasionAddDto.PersonId && p.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {occasionAddDto.PersonId}");

        if (occasionAddDto.Type != OccasionType.Custom)
        {
            var alreadyExists = await dbContext.Occasions
                .AnyAsync(o => o.PersonId == person.Id && o.Type == occasionAddDto.Type);

            if (alreadyExists)
            {
                throw new ConflictException(
                    $"{Translations.GetOccasionTypeDisplayText(occasionAddDto.Type)} ist für diese Person bereits angelegt.");
            }
        }

        var occasion = occasionAddDto.Type switch
        {
            OccasionType.Birthday => CreateBirthday(person),
            OccasionType.Christmas => CreateChristmas(person.Id),
            _ => CreateCustom(occasionAddDto)
        };

        dbContext.Occasions.Add(occasion);
        await dbContext.SaveChangesAsync();

        return occasion;
    }

    private static Occasion CreateBirthday(Person person)
    {
        if (!person.DateOfBirth.HasValue)
        {
            throw new ConflictException("Für den Anlass Geburtstag muss bei der Person ein Geburtsdatum hinterlegt sein.");
        }

        return new Occasion(OccasionType.Birthday, null, person.DateOfBirth.Value, true, person.Id);
    }

    private static Occasion CreateChristmas(int personId) =>
        new(OccasionType.Christmas, null, new DateOnly(DateTime.Today.Year, 12, 24), true, personId);

    private static Occasion CreateCustom(OccasionAddDto occasionAddDto)
    {
        if (string.IsNullOrWhiteSpace(occasionAddDto.Title))
        {
            throw new ConflictException("Für einen benutzerdefinierten Anlass ist ein Titel erforderlich.");
        }

        return new Occasion(
            OccasionType.Custom,
            occasionAddDto.Title.Trim(),
            occasionAddDto.Date,
            occasionAddDto.IsRecurring,
            occasionAddDto.PersonId);
    }

    public async Task<Occasion> UpdateOccasionAsync(int occasionId, int currentUserId, OccasionUpdateDto occasionUpdateDto)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var occasion = await dbContext.Occasions
            .Where(o => o.Id == occasionId && o.Person.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Anlass konnte nicht gefunden werden: {occasionId}");

        if (occasion.Type != OccasionType.Custom)
        {
            throw new ConflictException("Feste Anlässe können nicht bearbeitet werden.");
        }

        if (string.IsNullOrWhiteSpace(occasionUpdateDto.Title))
        {
            throw new ConflictException("Für einen benutzerdefinierten Anlass ist ein Titel erforderlich.");
        }

        occasion.Update(new OccasionUpdateDto
        {
            Title = occasionUpdateDto.Title.Trim(),
            Date = occasionUpdateDto.Date,
            IsRecurring = occasionUpdateDto.IsRecurring
        });

        await dbContext.SaveChangesAsync();

        return occasion;
    }

    public async Task DeleteOccasionAsync(int occasionId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var occasion = await dbContext.Occasions
            .Where(o => o.Id == occasionId && o.Person.UserId == currentUserId)
            .Include(o => o.Gifts)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Anlass konnte nicht gefunden werden: {occasionId}");

        var occasionLabel = Translations.GetOccasionDisplayTitle(occasion);
        var occasionYear = occasion.GetNextOccurrence(DateOnly.FromDateTime(DateTime.Today)).Year;

        foreach (var gift in occasion.Gifts)
        {
            gift.DetachFromOccasion(occasionLabel, occasionYear);
        }

        dbContext.Occasions.Remove(occasion);
        await dbContext.SaveChangesAsync();
    }
}
