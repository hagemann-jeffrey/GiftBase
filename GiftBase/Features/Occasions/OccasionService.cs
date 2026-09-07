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
}
