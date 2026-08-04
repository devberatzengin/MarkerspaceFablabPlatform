namespace MakerspaceFablabPlatform.Events;

// BU bizim observer'ımız
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
