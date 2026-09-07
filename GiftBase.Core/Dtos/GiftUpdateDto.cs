namespace GiftBase.Core.Dtos;

public class GiftUpdateDto
{
    public string Title { get; set; } = null!;
    public string? Note { get; set; }
    public string? Link { get; set; }
    public decimal? Price { get; set; }
    public Enums.GiftStatus Status { get; set; }
    public int? OccasionId { get; set; }
    public string? OccasionLabel { get; set; }
    public int? OccasionYear { get; set; }
}