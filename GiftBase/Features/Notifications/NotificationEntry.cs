using GiftBase.Core.Entities;

namespace GiftBase.Features.Notifications;

public class NotificationEntry
{
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public Occasion Occasion { get; set; } = null!;
    public IReadOnlyList<string> GiftIdeas { get; set; } = [];
}
