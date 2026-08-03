namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{ 
    IAnnouncementRepository Announcements { get; }
    ICategoryRepository Categories { get; }
    IUserRepository Users { get; }
    IEquipmentRepository Equipments { get; }
    IEquipmentRentalRepository EquipmentRentals { get; }
    IPaymentRepository Payments { get; }
    ISubscriptionRepository Subscriptions { get; }
    INotificationRepository Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(Guid userId, CancellationToken ct = default);
}