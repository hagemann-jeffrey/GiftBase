using GiftBase.Core.Dtos.Gifts;
using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IGiftService
{
    Task<List<Gift>> GetGiftsAsync(int personId, int currentUserId);
    Task<Gift> AddGiftAsync(GiftAddDto giftAddDto, int currentUserId);
    Task<Gift> UpdateGiftAsync(int giftId, int currentUserId, GiftUpdateDto giftUpdateDto);
    Task DeleteGiftAsync(int giftId, int currentUserId);
    Task<Dictionary<int, int>> GetGiftCountsByPersonAsync(int currentUserId);
    Task<GiftImage> GetGiftImageAsync(int giftId, int currentUserId);
}