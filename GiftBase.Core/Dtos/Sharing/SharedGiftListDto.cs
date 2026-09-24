namespace GiftBase.Core.Dtos.Sharing;

public class SharedGiftListDto
{
    public string RecipientFirstName { get; set; } = null!;
    public string? OccasionTitle { get; set; }
    public List<SharedGiftDto> Gifts { get; set; } = [];
}
