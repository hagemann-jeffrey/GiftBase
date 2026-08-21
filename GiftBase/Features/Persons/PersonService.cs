using GiftBase.Core.Entities;
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
}