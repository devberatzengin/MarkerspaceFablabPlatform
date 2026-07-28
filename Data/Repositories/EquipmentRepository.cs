using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class EquipmentRepository : Repository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(IApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<User?> GetCurrentUserAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await DbContext.EquipmentRentals
            .Where(r => r.EquipmentId == equipmentId && r.ReleasedAt == null)
            .Select(r => r.User)
            .FirstOrDefaultAsync(cancellationToken);
    }
}