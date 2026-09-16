using GiftBase.Core.Dtos;
using GiftBase.Core.Enums;

namespace GiftBase.Core.Entities;

public class Gift
{
    private Gift() { }

    public Gift(string title, string? note, string? link, decimal? price, int personId, int? occasionId = null)
    {
        Title = title;
        Note = note;
        Link = link;
        Price = price;
        Status = GiftStatus.Idea;
        PersonId = personId;
        OccasionId = occasionId;
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Note { get; private set; }
    public string? Link { get; private set; }
    public decimal? Price { get; private set; }
    public GiftStatus Status { get; private set; }
    public int PersonId { get; private set; }
    public int? OccasionId { get; private set; }
    public string? OccasionLabel { get; private set; }
    public int? OccasionYear { get; private set; }
    public Guid? ImageVersion { get; private set; }

    public Person Person { get; private set; } = null!;
    public Occasion? Occasion { get; private set; }

    public void Update(GiftUpdateDto giftUpdateDto)
    {
        Title = giftUpdateDto.Title;
        Note = giftUpdateDto.Note;
        Link = giftUpdateDto.Link;
        Price = giftUpdateDto.Price;
        Status = giftUpdateDto.Status;
        OccasionId = giftUpdateDto.OccasionId;

        if (giftUpdateDto.OccasionId.HasValue)
        {
            OccasionLabel = null;
            OccasionYear = null;
        }
    }

    public void DetachFromOccasion(string occasionLabel, int occasionYear)
    {
        OccasionId = null;
        OccasionLabel = occasionLabel;
        OccasionYear = occasionYear;
    }

    public void AttachImage() => ImageVersion = Guid.NewGuid();

    public void DetachImage() => ImageVersion = null;
}