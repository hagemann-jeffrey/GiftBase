using GiftBase.Core.Dtos.Gifts;
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

        await EnsureOccasionBelongsToPersonAsync(dbContext, giftAddDto.OccasionId, giftAddDto.PersonId);

        var gift = new Gift(
            giftAddDto.Title,
            giftAddDto.Note,
            giftAddDto.Link,
            giftAddDto.Price,
            giftAddDto.PersonId,
            giftAddDto.OccasionId
        );

        dbContext.Gifts.Add(gift);
        await dbContext.SaveChangesAsync();

        if (giftAddDto.ImageContent is not null)
        {
            EnsureImageIsValid(giftAddDto.ImageContent, giftAddDto.ImageContentType);

            dbContext.GiftImages.Add(new GiftImage(gift.Id, giftAddDto.ImageContentType!, giftAddDto.ImageContent));
            gift.AttachImage();

            await dbContext.SaveChangesAsync();
        }

        return gift;
    }

    public async Task<Gift> UpdateGiftAsync(int giftId, int currentUserId, GiftUpdateDto giftUpdateDto)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var gift = await dbContext.Gifts
            .Where(g => g.Id == giftId && g.Person.UserId == currentUserId)
            .SingleOrNotFoundAsync($"Geschenkidee konnte nicht gefunden werden: {giftId}");

        await EnsureOccasionBelongsToPersonAsync(dbContext, giftUpdateDto.OccasionId, gift.PersonId);

        gift.Update(giftUpdateDto);

        if (giftUpdateDto.ImageContent is not null)
        {
            EnsureImageIsValid(giftUpdateDto.ImageContent, giftUpdateDto.ImageContentType);

            var giftImage = await dbContext.GiftImages.SingleOrDefaultAsync(i => i.GiftId == gift.Id);

            if (giftImage is null)
            {
                dbContext.GiftImages.Add(new GiftImage(gift.Id, giftUpdateDto.ImageContentType!, giftUpdateDto.ImageContent));
            }
            else
            {
                giftImage.Replace(giftUpdateDto.ImageContentType!, giftUpdateDto.ImageContent);
            }

            gift.AttachImage();
        }
        else if (!giftUpdateDto.ImageVersion.HasValue)
        {
            var giftImage = await dbContext.GiftImages.SingleOrDefaultAsync(i => i.GiftId == gift.Id);

            if (giftImage is not null)
            {
                dbContext.GiftImages.Remove(giftImage);
            }

            gift.DetachImage();
        }

        await dbContext.SaveChangesAsync();

        return gift;
    }

    public async Task DeleteGiftAsync(int giftId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var gift = await dbContext.Gifts
            .Where(g => g.Id == giftId && g.Person.UserId == currentUserId)
            .SingleOrNotFoundAsync($"Geschenkidee konnte nicht gefunden werden: {giftId}");

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

    public async Task<GiftImage> GetGiftImageAsync(int giftId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var giftExists = await dbContext.Gifts
            .AnyAsync(g => g.Id == giftId && g.Person.UserId == currentUserId);

        var giftImage = giftExists
            ? await dbContext.GiftImages.SingleOrDefaultAsync(i => i.GiftId == giftId)
            : null;

        return giftImage ?? throw new NotFoundException($"Bild konnte nicht gefunden werden: {giftId}");
    }

    private static void EnsureImageIsValid(byte[] content, string? contentType)
    {
        if (content.LongLength > GiftImage.MaxContentLength)
        {
            throw new ConflictException("Das Bild darf maximal 5 MB groß sein.");
        }

        if (!GiftImage.IsSupportedContentType(contentType))
        {
            throw new ConflictException("Nur JPG- und PNG-Bilder werden unterstützt.");
        }
    }

    private static async Task EnsureOccasionBelongsToPersonAsync(GiftBaseDbContext dbContext, int? occasionId, int personId)
    {
        if (!occasionId.HasValue)
        {
            return;
        }

        var occasionExists = await dbContext.Occasions
            .AnyAsync(o => o.Id == occasionId.Value && o.PersonId == personId);

        if (!occasionExists)
        {
            throw new NotFoundException($"Anlass konnte nicht gefunden werden: {occasionId.Value}");
        }
    }
}