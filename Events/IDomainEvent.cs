namespace MakerspaceFablabPlatform.Events;

// Şimbi bu bir event 
// Ne olabilir mesela
//      - Duyuru yayınladnı 
//      - Duyuru yayından kaldırıldı cart curt türlerini ayrı ayrı tanımlıyorsun mesela


public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}