using GiftBase.Core.Dtos.Notifications;

namespace GiftBase.Core.Interfaces;

public interface INotificationService
{
    Task<NotificationDispatchResultDto> DispatchAsync(DateOnly today, string baseUrl, bool dryRun);
}
