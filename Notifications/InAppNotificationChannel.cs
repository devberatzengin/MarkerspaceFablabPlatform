using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities.Enums;
using NotificationEntity = MakerspaceFablabPlatform.Entities.Notification;
using NotificationType = MakerspaceFablabPlatform.Entities.Enums.NotificationType;

namespace MakerspaceFablabPlatform.Notifications;

public class InAppNotificationChannel : INotificationChannel
{
    private readonly IUnitOfWork _unitOfWork;
    
    public NotificationChannelType Type => NotificationChannelType.InApp;

    public InAppNotificationChannel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task SendAsync(NotificationMessage message, CancellationToken token)
    {
        var notification = Translate(message);

        await _unitOfWork.Notifications.AddAsync(notification, token);
    }

    private NotificationEntity Translate(NotificationMessage message)
    {
        return new NotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = message.UserId,
            Type = message.Type,
            Channel = this.Type,
            Title = message.Title,
            Message = message.Message,
            RelatedEntityId = message.RelatedEntityId,
            ThresholdMinutes = message.ThresholdMinutes,

            IsRead = false,
            ReadAt = null,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}