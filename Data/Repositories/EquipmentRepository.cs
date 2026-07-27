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
    
    public async Task<User?> GetEquipmentsUser(Equipment equipment, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(e => e.Id == equipment.Id)
            .Select(e => e.UsingBy)
            .FirstOrDefaultAsync(cancellationToken);
    }
}