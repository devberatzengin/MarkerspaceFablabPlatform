using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Services;

namespace MakerspaceFablabPlatform.Notifications;

public class SmtpEmailChannel : INotificationChannel
{
    private readonly SmtpService _smtpService;

    public NotificationChannelType Type => NotificationChannelType.Email;

    public SmtpEmailChannel(SmtpService smtpService)
    {
        _smtpService = smtpService;
    }

    public async Task SendAsync(NotificationMessage message, CancellationToken token)
    {
        
        if (string.IsNullOrWhiteSpace(message.Email))
            throw new InvalidOperationException($"Kullanıcı {message.UserId} için e-posta adresi yok.");

        await _smtpService.SendAsync(message.Email, message.Title, message.Message, token);
    }
}