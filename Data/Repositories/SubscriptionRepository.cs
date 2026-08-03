using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(IApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Subscription>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Include(s => s.User)
            .Where(s => s.CategoryId == categoryId && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Include(s => s.User)
            .Where(s => s.EquipmentId == equipmentId && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Include(s => s.Category)
            .Include(s => s.Equipment)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query(asNoTracking: false)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsForCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return Query().AnyAsync(s => s.UserId == userId && s.CategoryId == categoryId, cancellationToken);
    }

    public Task<bool> ExistsForEquipmentAsync(Guid userId, Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return Query().AnyAsync(s => s.UserId == userId && s.EquipmentId == equipmentId, cancellationToken);
    }
}
