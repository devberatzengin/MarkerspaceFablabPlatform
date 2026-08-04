namespace MakerspaceFablabPlatform.Events;

public record AnnouncementPublishedEvent(
    Guid AnnouncementId,
    Guid CategoryId,
    string Title,
    DateTime OccurredOn) : IDomainEvent;
