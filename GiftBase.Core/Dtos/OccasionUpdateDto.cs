namespace GiftBase.Core.Dtos;

public class OccasionUpdateDto
{
    public string? Title { get; set; }
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
}
