using System.Buffers.Text;
using System.Security.Cryptography;
using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.Sharing;

public class ShareLinkService(IDbContextFactory<GiftBaseDbContext> dbContextFactory) : IShareLinkService
{
    public async Task<List<ShareLink>> GetShareLinksAsync(int personId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.ShareLinks
            .Where(s => s.PersonId == personId && s.UserId == currentUserId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShareLink> AddShareLinkAsync(ShareLinkAddDto shareLinkAddDto, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var personExists = await dbContext.Persons
            .AnyAsync(p => p.Id == shareLinkAddDto.PersonId && p.UserId == currentUserId);

        if (!personExists)
        {
            throw new NotFoundException($"Person konnte nicht gefunden werden: {shareLinkAddDto.PersonId}");
        }

        if (shareLinkAddDto.OccasionId.HasValue)
        {
            var occasionExists = await dbContext.Occasions
                .AnyAsync(o => o.Id == shareLinkAddDto.OccasionId.Value && o.PersonId == shareLinkAddDto.PersonId);

            if (!occasionExists)
            {
                throw new NotFoundException($"Anlass konnte nicht gefunden werden: {shareLinkAddDto.OccasionId.Value}");
            }
        }

        var createdAt = DateTime.UtcNow;
        var oldestValidCreation = createdAt - ShareLink.Lifetime;

        var validLinkExists = await dbContext.ShareLinks
            .AnyAsync(s => s.UserId == currentUserId
                && s.PersonId == shareLinkAddDto.PersonId
                && s.OccasionId == shareLinkAddDto.OccasionId
                && s.CreatedAt > oldestValidCreation);

        if (validLinkExists)
        {
            throw new ConflictException("Für diesen Umfang existiert bereits ein gültiger Link.");
        }

        var shareLink = new ShareLink(
            GenerateToken(),
            currentUserId,
            shareLinkAddDto.PersonId,
            shareLinkAddDto.OccasionId,
            createdAt);

        dbContext.ShareLinks.Add(shareLink);
        await dbContext.SaveChangesAsync();

        return shareLink;
    }

    public async Task DeleteShareLinkAsync(int shareLinkId, int currentUserId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var shareLink = await dbContext.ShareLinks
            .Where(s => s.Id == shareLinkId && s.UserId == currentUserId)
            .SingleOrDefaultAsync()
                ?? throw new NotFoundException($"Link konnte nicht gefunden werden: {shareLinkId}");

        dbContext.ShareLinks.Remove(shareLink);
        await dbContext.SaveChangesAsync();
    }

    public Task<SharedGiftListDto> GetSharedGiftListAsync(string token) =>
        throw new NotImplementedException();

    private static string GenerateToken() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
}
