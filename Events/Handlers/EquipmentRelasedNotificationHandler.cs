using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Notifications;

namespace MakerspaceFablabPlatform.Events.Handlers;

public class EquipmentRelasedNotificationHandler : IDomainEventHandler<EquipmentRelasedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationChannelFactory _channelFactory;
    private readonly ILogger<AnnouncementPublishedNotificationHandler> _logger;

    public EquipmentRelasedNotificationHandler(IUnitOfWork unitOfWork,INotificationChannelFactory channelFactory, ILogger<AnnouncementPublishedNotificationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _channelFactory = channelFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(EquipmentRelasedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _unitOfWork.Subscriptions
            .GetByEquipmentIdAsync(domainEvent.EquipmentId, cancellationToken);

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("Not found any subscriptions for {EquipmentId}, not created any notification(s)", domainEvent.EquipmentId);
        }

        var sentCount = 0;

        foreach (var subscription in subscriptions)
        {
            try
            {
                var channel = _channelFactory.Create(subscription.Channel);

                var message = new NotificationMessage
                {
                    UserId = subscription.UserId,
                    Email = subscription.User.Email,
                    Type = NotificationType.EquipmentAvailable,
                    Title = "Takip ettiğiniz Equipment şuan müsait",
                    Message = $"\"{domainEvent.Title}\" başlıklı yeni bir duyuru yayınlandı.",
                    RelatedEntityId = domainEvent.EquipmentId
                };
                
                await channel.SendAsync(message, cancellationToken);
                
                sentCount++;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Subscriber {UserId} cant get the notification. Channel: {Channel}, EquipmentId: {EquipmentId}",
                    subscription.UserId, subscription.Channel, domainEvent.EquipmentId);
            }
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        
        _logger.LogInformation("{SentCount}/{TotalCount} subscriber gets the notification. EquipmentId: {EquipmentId}",
            sentCount, subscriptions.Count, domainEvent.EquipmentId);
    }
}