using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Notifications;

public interface INotificationChannelFactory
{
    INotificationChannel Create(NotificationChannelType type);
}