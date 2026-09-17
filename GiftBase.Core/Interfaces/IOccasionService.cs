using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IOccasionService
{
    Task<List<Occasion>> GetOccasionsAsync(int personId, int currentUserId);
    Task<Dictionary<int, Occasion>> GetNextOccasionsByPersonAsync(int currentUserId);
    Task<Occasion> AddOccasionAsync(OccasionAddDto occasionAddDto, int currentUserId);
    Task<Occasion> UpdateOccasionAsync(int occasionId, int currentUserId, OccasionUpdateDto occasionUpdateDto);
    Task DeleteOccasionAsync(int occasionId, int currentUserId);
}
