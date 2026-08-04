namespace MakerspaceFablabPlatform.Events;

public record EquipmentRelasedEvent (
    Guid EquipmentId,
    string Title,
    Guid RelatedEntityId,
    DateTime OccurredOn): IDomainEvent;