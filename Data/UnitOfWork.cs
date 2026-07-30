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

    public UnitOfWork(
        AppDbContext dbContext,
        IPaymentRepository paymentRepository,
        IAnnouncementRepository announcementRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IEquipmentRepository equipmentRepository,
        IEquipmentRentalRepository equipmentRentalRepository)
    {
        _paymentRepository = paymentRepository;
        _dbContext = dbContext;
        _announcementRepository = announcementRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _equipmentRepository = equipmentRepository;
        _equipmentRentalRepository = equipmentRentalRepository;
    }

    public IAnnouncementRepository Announcements => _announcementRepository;
    public ICategoryRepository Categories => _categoryRepository;
    public IUserRepository Users => _userRepository;
    public IEquipmentRepository Equipments => _equipmentRepository;
    public IEquipmentRentalRepository EquipmentRentals => _equipmentRentalRepository;
    public IPaymentRepository Payments => _paymentRepository;

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