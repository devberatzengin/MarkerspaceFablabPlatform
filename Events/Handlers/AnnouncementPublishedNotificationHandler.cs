using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Notifications;

namespace MakerspaceFablabPlatform.Events.Handlers;


// Bu bizim olayımız.
// örnek olarak bu olayımız şu => tekip edilen kategoriye ait bir duyuru published olursa bunlar yaşansın diyoruz git status

public class AnnouncementPublishedNotificationHandler : IDomainEventHandler<AnnouncementPublishedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationChannelFactory _channelFactory;
    private readonly ILogger<AnnouncementPublishedNotificationHandler> _logger;

    public AnnouncementPublishedNotificationHandler(IUnitOfWork unitOfWork, INotificationChannelFactory channelFactory, ILogger<AnnouncementPublishedNotificationHandler> logger)
    {
        
        _unitOfWork = unitOfWork;
        _channelFactory = channelFactory;
        _logger = logger;
    }

    public async Task HandleAsync(AnnouncementPublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _unitOfWork.Subscriptions
            .GetByCategoryIdAsync(domainEvent.CategoryId, cancellationToken);

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("Not found any subscriptions for {CategoryId}, not created any notification(s)", domainEvent.CategoryId);
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
                    Type = NotificationType.AnnouncementPublished,
                    Title = $"Takip ettiğiniz kategoride yeni duyuru",
                    Message = $"\"{domainEvent.Title}\" başlıklı yeni bir duyuru yayınlandı.",
                    RelatedEntityId = domainEvent.AnnouncementId
                };

                await channel.SendAsync(message, cancellationToken);

                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Subscriber {UserId} cant get the notification. Channel: {Channel}, AnnouncementId: {AnnouncementId}",
                    subscription.UserId, subscription.Channel, domainEvent.AnnouncementId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{SentCount}/{TotalCount} subscriber gets the notification. AnnouncementId: {AnnouncementId}",
            sentCount, subscriptions.Count, domainEvent.AnnouncementId);
    }
}


