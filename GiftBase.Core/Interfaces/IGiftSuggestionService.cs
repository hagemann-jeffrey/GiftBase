using GiftBase.Core.Dtos;

namespace GiftBase.Core.Interfaces;

public interface IGiftSuggestionService
{
    Task<List<GiftSuggestionDto>> GenerateAsync(GiftSuggestionGenerateDto giftSuggestionGenerateDto, int currentUserId);
}
