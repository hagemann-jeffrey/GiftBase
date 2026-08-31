using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.Gifts;

public class GiftService(IDbContextFactory<GiftBaseDbContext> dbContextFactory) : IGiftService
{
    public async Task<List<Gift>> GetGiftsAsync(int personId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Gifts
            .Where(g => g.PersonId == personId && g.Person.UserId == currentUserId)
            .ToListAsync();
    }

    public async Task<Gift> AddGiftAsync(GiftAddDto giftAddDto, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var personExists = await dbContext.Persons
            .AnyAsync(p => p.Id == giftAddDto.PersonId && p.UserId == currentUserId);

        if (!personExists)
        {
            throw new NotFoundException($"Person konnte nicht gefunden werden: {giftAddDto.PersonId}");
        }

        var gift = new Gift(
            giftAddDto.Title,
            giftAddDto.Note,
            giftAddDto.Link,
            giftAddDto.Price,
            giftAddDto.PersonId
        );

        dbContext.Gifts.Add(gift);
        await dbContext.SaveChangesAsync();

        return gift;
    }

    public async Task<Gift> UpdateGiftAsync(int giftId, int currentUserId, GiftUpdateDto giftUpdateDto)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var gift = await dbContext.Gifts
            .Where(g => g.Id == giftId && g.Person.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Geschenkidee konnte nicht gefunden werden: {giftId}");

        gift.Update(giftUpdateDto);

        await dbContext.SaveChangesAsync();

        return gift;
    }

    public async Task DeleteGiftAsync(int giftId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var gift = await dbContext.Gifts
            .Where(g => g.Id == giftId && g.Person.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Geschenkidee konnte nicht gefunden werden: {giftId}");

        dbContext.Gifts.Remove(gift);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> GetGiftCountsByPersonAsync(int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Persons
            .Where(p => p.UserId == currentUserId)
            .Select(p => new { p.Id, GiftCount = p.Gifts.Count })
            .ToDictionaryAsync(x => x.Id, x => x.GiftCount);
    }
}