using GiftBase.Core.Dtos;

namespace GiftBase.Core.Interfaces;

public interface INotificationService
{
    Task<NotificationDispatchResultDto> DispatchAsync(DateOnly today, string baseUrl, bool dryRun);
}
