using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface IEquipmentRentalRepository : IRepository<EquipmentRental>
{
    // Ekipmanı fiilen elinde tutan kaydı döndürür: penceresi başlamış ve henüz iade edilmemiş kayıtların en eskisi. 
    Task<EquipmentRental?> GetCurrentRentalAsync(Guid equipmentId, DateTime beforeOfThisTime, CancellationToken cancellationToken = default);
    //  kullanıcının, bu ekipman için penceresi başlamış ve henüz iade edilmemiş kaydını döndürür.
    Task<EquipmentRental?> GetOpenRentalForUserAsync(Guid equipmentId, Guid userId, DateTime at, CancellationToken cancellationToken = default);

    Task<bool> IsAlreadyTakenThisTimespan(Guid equipmentId, DateTime start, DateTime end, Guid? excludeRentalId = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<EquipmentRental>> GetActiveRentalsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentRental>> GetRentalHistoryAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<EquipmentRental?> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<int> GetActiveRentalCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);

}
