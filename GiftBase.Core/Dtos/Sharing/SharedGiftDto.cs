namespace GiftBase.Core.Dtos.Sharing;

public class SharedGiftDto
{
    public string Title { get; set; } = null!;
    public string? Note { get; set; }
    public decimal? Price { get; set; }
    public string? Link { get; set; }
}
