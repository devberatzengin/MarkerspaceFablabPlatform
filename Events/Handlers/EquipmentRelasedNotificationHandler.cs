using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Notifications;

namespace MakerspaceFablabPlatform.Events.Handlers;

public class EquipmentRelasedNotificationHandler : IDomainEventHandler<EquipmentRelasedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationChannelFactory _channelFactory;
    private readonly ILogger<EquipmentRelasedNotificationHandler> _logger;

    public EquipmentRelasedNotificationHandler(IUnitOfWork unitOfWork,INotificationChannelFactory channelFactory, ILogger<EquipmentRelasedNotificationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _channelFactory = channelFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(EquipmentRelasedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var allSubscriptions = await _unitOfWork.Subscriptions
            .GetByEquipmentIdAsync(domainEvent.EquipmentId, cancellationToken);

        // Ekipmanı iade eden kişiye "müsait oldu" bildirimi göndermenin anlamı yok.
        // ?? Mantıklı benim aklıma gelmemişti.
        var subscriptions = domainEvent.ReleasedByUserId is null
            ? allSubscriptions
            : allSubscriptions.Where(s => s.UserId != domainEvent.ReleasedByUserId.Value).ToList();

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("Not found any subscriptions for {EquipmentId}, not created any notification(s)", domainEvent.EquipmentId);
            return;
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
                    Title = "Takip ettiğiniz ekipman şu an müsait",
                    Message = $"Takip ettiğiniz \"{domainEvent.Title}\" iade edildi ve şu an kiralanabilir.",
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