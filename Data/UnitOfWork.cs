using MakerspaceFablabPlatform.Data.Interfaces;

namespace MakerspaceFablabPlatform.Data;

public class UnitOfWork : IUnitOfWork
{
    
    private readonly AppDbContext _dbContext;
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentRentalRepository _equipmentRentalRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly INotificationRepository _notificationRepository;

    public UnitOfWork(
        AppDbContext dbContext,
        IPaymentRepository paymentRepository,
        IAnnouncementRepository announcementRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IEquipmentRepository equipmentRepository,
        IEquipmentRentalRepository equipmentRentalRepository,
        ISubscriptionRepository subscriptionRepository,
        INotificationRepository notificationRepository)
    {
        _paymentRepository = paymentRepository;
        _dbContext = dbContext;
        _announcementRepository = announcementRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _equipmentRepository = equipmentRepository;
        _equipmentRentalRepository = equipmentRentalRepository;
        _subscriptionRepository = subscriptionRepository;
        _notificationRepository = notificationRepository;
    }

    public IAnnouncementRepository Announcements => _announcementRepository;
    public ICategoryRepository Categories => _categoryRepository;
    public IUserRepository Users => _userRepository;
    public IEquipmentRepository Equipments => _equipmentRepository;
    public IEquipmentRentalRepository EquipmentRentals => _equipmentRentalRepository;
    public IPaymentRepository Payments => _paymentRepository;
    public ISubscriptionRepository Subscriptions => _subscriptionRepository;
    public INotificationRepository Notifications => _notificationRepository;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
    
    // Bu kısım audit log gibi sistemlerde kullanılırmış.
    public async Task<int> SaveChangesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
    
    
    
    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
    
}