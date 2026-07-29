using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class EquipmentRentalRepository : Repository<EquipmentRental>, IEquipmentRentalRepository
{
    public EquipmentRentalRepository(IApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<EquipmentRental?> GetActiveRentalAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(r => r.EquipmentId == equipmentId && r.ReleasedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EquipmentRental>> GetActiveRentalsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(r => r.UserId == userId && r.ReleasedAt == null)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetActiveRentalCountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .CountAsync(r => r.UserId == userId && r.ReleasedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<EquipmentRental>> GetRentalHistoryAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(r => r.EquipmentId == equipmentId)
            .OrderByDescending(r => r.RentedAt)
            .ToListAsync(cancellationToken);
    }

    
    public async Task<EquipmentRental?> GetActiveByEquipmentIdAsync(Guid equipmentId, CancellationToken token)
    {
        return await Query()
            .Where(er => er.EquipmentId == equipmentId && er.ReleasedAt == null)
            .FirstOrDefaultAsync(cancellationToken: token);
    }
    
    public async Task<EquipmentRental?> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(r => r.EquipmentId == equipmentId, cancellationToken);
    }
}
