using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Data.Repositories;

namespace MakerspaceFablabPlatform.Data;

public class UnitOfWork : IUnitOfWork
{
    
    private readonly AppDbContext _dbContext;
    private readonly ICategoryRepository _categoryRepository;

    public UnitOfWork(AppDbContext dbContext, ICategoryRepository categoryRepository)
    {
        _dbContext = dbContext;
        _categoryRepository = categoryRepository;
    }

    private IAnnouncementRepository? _announcementRepository;
    private IUserRepository? _userRepository;
    private IEquipmentRepository? _equipmentRepository;

    public IAnnouncementRepository Announcements =>
        _announcementRepository ??= new AnnouncementRepository(_dbContext);

    public ICategoryRepository Categories => _categoryRepository;

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_dbContext);

    public IEquipmentRepository Equipments =>
        _equipmentRepository ??= new EquipmentRepository(_dbContext);

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