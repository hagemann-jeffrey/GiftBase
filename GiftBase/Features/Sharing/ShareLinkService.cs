using GiftBase.Core.Dtos.Sharing;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using GiftBase.Shared.Common;
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
            TokenGenerator.Generate(),
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
            .SingleOrNotFoundAsync($"Link konnte nicht gefunden werden: {shareLinkId}");

        dbContext.ShareLinks.Remove(shareLink);
        await dbContext.SaveChangesAsync();
    }

    public async Task<SharedGiftListDto> GetSharedGiftListAsync(string token)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var shareLink = await dbContext.ShareLinks
            .Where(s => s.Token == token)
            .SingleOrNotFoundAsync("Link konnte nicht gefunden werden.");

        if (shareLink.IsExpired(DateTime.UtcNow))
        {
            throw new NotFoundException("Link konnte nicht gefunden werden.");
        }

        var person = await dbContext.Persons
            .Where(p => p.Id == shareLink.PersonId && p.UserId == shareLink.UserId)
            .SingleOrNotFoundAsync("Link konnte nicht gefunden werden.");

        string? occasionTitle = null;

        if (shareLink.OccasionId.HasValue)
        {
            var occasion = await dbContext.Occasions
                .Where(o => o.Id == shareLink.OccasionId.Value && o.PersonId == person.Id)
                .SingleOrNotFoundAsync("Link konnte nicht gefunden werden.");

            occasionTitle = Translations.GetOccasionDisplayTitle(occasion);
        }

        var giftsQuery = dbContext.Gifts
            .Where(g => g.PersonId == person.Id && g.Status == GiftStatus.Idea);

        if (shareLink.OccasionId.HasValue)
        {
            giftsQuery = giftsQuery.Where(g => g.OccasionId == shareLink.OccasionId.Value);
        }

        var gifts = await giftsQuery
            .OrderBy(g => g.Title)
            .Select(g => new SharedGiftDto
            {
                Title = g.Title,
                Note = g.Note,
                Price = g.Price,
                Link = g.Link
            })
            .ToListAsync();

        return new SharedGiftListDto
        {
            RecipientFirstName = person.FirstName,
            OccasionTitle = occasionTitle,
            Gifts = gifts
        };
    }
}
