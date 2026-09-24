namespace GiftBase.Core.Dtos.Notifications;

public class NotificationDispatchResultDto
{
    public DateOnly Today { get; set; }
    public bool DryRun { get; set; }
    public List<NotificationDispatchDto> Dispatches { get; set; } = [];
}
