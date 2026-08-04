using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Notifications;

public interface INotificationChannel
{
    public NotificationChannelType Type { get; }

    public Task SendAsync(NotificationMessage message, CancellationToken token);
}