using GiftBase.Core.Dtos.GiftSuggestions;

namespace GiftBase.Core.Interfaces;

public interface IGiftSuggestionService
{
    Task<List<GiftSuggestionDto>> GenerateAsync(GiftSuggestionGenerateDto giftSuggestionGenerateDto, int currentUserId);
}
