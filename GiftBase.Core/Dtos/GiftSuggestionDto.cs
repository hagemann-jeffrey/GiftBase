namespace GiftBase.Core.Dtos;

public class GiftSuggestionDto
{
    public Enums.GiftSuggestionType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string SearchTerm { get; set; } = null!;
    public decimal Price { get; set; }
}
