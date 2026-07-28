using MakerspaceFablabPlatform.Data.Interfaces;

namespace MakerspaceFablabPlatform.Data;

public class UnitOfWork : IUnitOfWork
{
    
    private readonly AppDbContext _dbContext;
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public UnitOfWork(
        AppDbContext dbContext,
        IAnnouncementRepository announcementRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IEquipmentRepository equipmentRepository)
    {
        _dbContext = dbContext;
        _announcementRepository = announcementRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _equipmentRepository = equipmentRepository;
    }

    public IAnnouncementRepository Announcements => _announcementRepository;
    public ICategoryRepository Categories => _categoryRepository;
    public IUserRepository Users => _userRepository;
    public IEquipmentRepository Equipments => _equipmentRepository;

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