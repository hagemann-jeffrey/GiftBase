namespace GiftBase.Core.Dtos;

public class OccasionAddDto
{
    public Enums.OccasionType Type { get; set; }
    public string? Title { get; set; }
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public int PersonId { get; set; }
}
