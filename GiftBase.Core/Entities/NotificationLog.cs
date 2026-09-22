using GiftBase.Core.Enums;

namespace GiftBase.Core.Entities;

public class NotificationLog
{
    private NotificationLog() { }

    public NotificationLog(int userId, NotificationKind kind, string periodKey, DateTime sentAtUtc)
    {
        UserId = userId;
        Kind = kind;
        PeriodKey = periodKey;
        SentAtUtc = sentAtUtc;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public NotificationKind Kind { get; private set; }
    public string PeriodKey { get; private set; } = null!;
    public DateTime SentAtUtc { get; private set; }
}
