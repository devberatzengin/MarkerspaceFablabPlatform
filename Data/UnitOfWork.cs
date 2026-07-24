using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Data.Repositories;

namespace MakerspaceFablabPlatform.Data;

public class UnitOfWork : IUnitOfWork
{
    
    private readonly AppDbContext _dbContext;
    public UnitOfWork(AppDbContext dbContext) { _dbContext = dbContext; }
    
    private IAnnouncementRepository? _announcementRepository;
    private ICategoryRepository? _categoryRepository;
    private IUserRepository? _userRepository;

    public IAnnouncementRepository Announcements => 
        _announcementRepository ??= new AnnouncementRepository(_dbContext);

    public ICategoryRepository Categories =>
        _categoryRepository ??= new CategoryRepository(_dbContext);
    
    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_dbContext);
    
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