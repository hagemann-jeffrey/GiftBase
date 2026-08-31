using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
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
            personAddDto.UserId
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
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {personId}");

        person.Update(personUpdateDto);

        await dbContext.SaveChangesAsync();

        return person;
    }

    public async Task DeletePersonAsync(int personId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var person = await dbContext.Persons
            .Where(p => p.Id == personId && p.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Person konnte nicht gefunden werden: {personId}");

        dbContext.Persons.Remove(person);
        await dbContext.SaveChangesAsync();
    }
}