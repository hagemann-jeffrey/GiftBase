namespace GiftBase.Core.Dtos.GiftSuggestions;

public class GiftSuggestionGenerateDto
{
    public int PersonId { get; set; }
    public int OccasionId { get; set; }
    public decimal Budget { get; set; }
    public string? AdditionalHint { get; set; }
}
