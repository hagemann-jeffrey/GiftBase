using GiftBase.Core.Dtos;
using GiftBase.Core.Enums;

namespace GiftBase.Core.Entities;

public class Gift
{
    private Gift() { }

    public Gift(string title, string? note, string? link, decimal? price, int personId)
    {
        Title = title;
        Note = note;
        Link = link;
        Price = price;
        Status = GiftStatus.Idea;
        PersonId = personId;
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Note { get; private set; }
    public string? Link { get; private set; }
    public decimal? Price { get; private set; }
    public GiftStatus Status { get; private set; }
    public int PersonId { get; private set; }

    public Person Person { get; private set; } = null!;

    public void Update(GiftUpdateDto giftUpdateDto)
    {
        Title = giftUpdateDto.Title;
        Note = giftUpdateDto.Note;
        Link = giftUpdateDto.Link;
        Price = giftUpdateDto.Price;
        Status = giftUpdateDto.Status;
    }
}