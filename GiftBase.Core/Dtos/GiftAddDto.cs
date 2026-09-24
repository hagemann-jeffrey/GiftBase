namespace GiftBase.Core.Dtos;

public class GiftAddDto
{
    public string Title { get; set; } = null!;
    public string? Note { get; set; }
    public string? Link { get; set; }
    public decimal? Price { get; set; }
    public int PersonId { get; set; }
    public int? OccasionId { get; set; }
    public byte[]? ImageContent { get; set; }
    public string? ImageContentType { get; set; }
}