using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IOccasionService
{
    Task<List<Occasion>> GetOccasionsAsync(int personId, int currentUserId);
    Task<Occasion> AddOccasionAsync(OccasionAddDto occasionAddDto, int currentUserId);
}
