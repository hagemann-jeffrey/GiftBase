using GiftBase.Core.Dtos.Persons;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.Persons;

public class PersonService(IDbContextFactory<GiftBaseDbContext> dbContextFactory) : IPersonService
{
    public async Task<List<Person>> GetPersonsAsync(int userId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Persons
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<Person> GetPersonAsync(int personId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Persons
            .Where(p => p.Id == personId && p.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {personId}");
    }

    public async Task<Person> AddPersonAsync(PersonAddDto personAddDto)
    {
        var person = new Person(
            personAddDto.FirstName,
            personAddDto.LastName,
            personAddDto.DateOfBirth,
            personAddDto.Relation,
            personAddDto.UserId,
            personAddDto.Interests,
            personAddDto.NotificationsEnabled
        );

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        return person;
    }

    public async Task<Person> UpdatePersonAsync(int personId, int currentUserId, PersonUpdateDto personUpdateDto)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var person = await dbContext.Persons
            .Where(p => p.Id == personId && p.UserId == currentUserId)
            .Include(p => p.Occasions)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {personId}");

        var birthday = person.Occasions.SingleOrDefault(o => o.Type == OccasionType.Birthday);

        if (birthday is not null && !personUpdateDto.DateOfBirth.HasValue)
        {
            throw new ConflictException("Das Geburtsdatum kann nicht entfernt werden, solange ein Geburtstags-Anlass besteht.");
        }

        person.Update(personUpdateDto);

        if (birthday is not null && personUpdateDto.DateOfBirth.HasValue)
        {
            birthday.SetDate(personUpdateDto.DateOfBirth.Value);
        }

        await dbContext.SaveChangesAsync();

        return person;
    }

    public async Task DeletePersonAsync(int personId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var person = await dbContext.Persons
            .Where(p => p.Id == personId && p.UserId == currentUserId)
            .Include(p => p.Gifts)
            .Include(p => p.Occasions)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {personId}");

        dbContext.Persons.Remove(person);
        await dbContext.SaveChangesAsync();
    }
}