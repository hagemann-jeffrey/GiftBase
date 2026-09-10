using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IShareLinkService
{
    Task<List<ShareLink>> GetShareLinksAsync(int personId, int currentUserId);
    Task<ShareLink> AddShareLinkAsync(ShareLinkAddDto shareLinkAddDto, int currentUserId);
    Task DeleteShareLinkAsync(int shareLinkId, int currentUserId);
    Task<SharedGiftListDto> GetSharedGiftListAsync(string token);
}
