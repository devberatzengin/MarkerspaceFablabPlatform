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

    public async Task<IReadOnlyList<EquipmentRental>> GetRentalHistoryAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(r => r.EquipmentId == equipmentId)
            .OrderByDescending(r => r.RentedAt)
            .ToListAsync(cancellationToken);
    }
}
