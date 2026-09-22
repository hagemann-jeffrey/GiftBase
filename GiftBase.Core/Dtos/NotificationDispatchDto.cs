using GiftBase.Core.Enums;

namespace GiftBase.Core.Dtos;

public class NotificationDispatchDto
{
    public string Recipient { get; set; } = null!;
    public NotificationKind Kind { get; set; }
    public string PeriodKey { get; set; } = null!;
    public string? Error { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
