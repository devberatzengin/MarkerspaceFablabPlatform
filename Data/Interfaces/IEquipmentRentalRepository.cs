using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface IEquipmentRentalRepository : IRepository<EquipmentRental>
{
    Task<EquipmentRental?> GetCurrentRentalAsync(Guid equipmentId, DateTime asOf, CancellationToken cancellationToken = default);

    Task<bool> IsAlreadyTakenThisTimespan(Guid equipmentId, DateTime start, DateTime end, Guid? excludeRentalId = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<EquipmentRental>> GetActiveRentalsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentRental>> GetRentalHistoryAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<EquipmentRental?> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<int> GetActiveRentalCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);

}
