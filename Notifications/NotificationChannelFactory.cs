using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Notifications;

public class NotificationChannelFactory : INotificationChannelFactory
{
    private readonly IReadOnlyDictionary<NotificationChannelType, INotificationChannel> _channels;

    public NotificationChannelFactory(IEnumerable<INotificationChannel> channels)
    {
        _channels = channels.ToDictionary(c => c.Type);
    }

    public INotificationChannel Create(NotificationChannelType type)
    {
        if (!_channels.TryGetValue(type, out var channel))
            throw new NotSupportedException($"'{type}' için tanımlı bir bildirim kanalı yok.");

        return channel;
    }
}
