using GiftBase.Core.Enums;

namespace GiftBase.Core.Dtos.GiftSuggestions;

public class GiftSuggestionDto
{
    public GiftSuggestionType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string SearchTerm { get; set; } = null!;
    public decimal Price { get; set; }
}
