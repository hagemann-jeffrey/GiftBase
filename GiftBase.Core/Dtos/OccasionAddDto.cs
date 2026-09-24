using GiftBase.Core.Enums;

namespace GiftBase.Core.Dtos;

public class OccasionAddDto
{
    public OccasionType Type { get; set; }
    public string? Title { get; set; }
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public int PersonId { get; set; }
}
