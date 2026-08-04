using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Notifications;

public class SmtpEmailChannel : INotificationChannel
{
    public NotificationChannelType Type => NotificationChannelType.Email;
    
    public Task SendAsync(NotificationMessage message, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}