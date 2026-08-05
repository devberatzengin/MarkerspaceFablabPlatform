using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class EquipmentRentalRepository : Repository<EquipmentRental>, IEquipmentRentalRepository
{
    public EquipmentRentalRepository(IApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<EquipmentRental?> GetCurrentRentalAsync(Guid equipmentId, DateTime beforeOfThisTime, CancellationToken cancellationToken = default)
    {
        // En eski açık kayıt = ekipmanı fiilen tutan taraf. Süresi dolmuş ama iade
        // edilmemiş bir kayıt da buraya düşer; sıradaki kişinin penceresi başlamış
        // olsa bile ekipman hâlâ gecikenin elindedir.
        return await Query()
            .Where(r => r.EquipmentId == equipmentId && r.ReleasedAt == null && r.RentedAt <= beforeOfThisTime)
            .OrderBy(r => r.RentedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EquipmentRental?> GetOpenRentalForUserAsync(Guid equipmentId, Guid userId, DateTime at, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(r => r.EquipmentId == equipmentId
                        && r.UserId == userId
                        && r.ReleasedAt == null
                        && r.RentedAt <= at)
            .OrderBy(r => r.RentedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsAlreadyTakenThisTimespan(Guid equipmentId, DateTime start, DateTime end, Guid? excludeRentalId = null, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AnyAsync(r => r.EquipmentId == equipmentId
                        && r.ReleasedAt == null
                        && (excludeRentalId == null || r.Id != excludeRentalId)
                        && start < r.ExpectedReturnAt
                        && r.RentedAt < end,
                      cancellationToken);
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
