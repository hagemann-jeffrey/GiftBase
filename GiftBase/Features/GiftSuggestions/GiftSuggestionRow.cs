using GiftBase.Core.Dtos;

namespace GiftBase.Features.GiftSuggestions;

public class GiftSuggestionRow(GiftSuggestionDto suggestion)
{
    public GiftSuggestionDto Suggestion { get; } = suggestion;
    public bool Selected { get; set; } = true;
}
