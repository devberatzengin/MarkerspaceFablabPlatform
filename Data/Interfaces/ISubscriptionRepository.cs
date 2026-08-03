using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<IReadOnlyList<Subscription>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Subscription?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForEquipmentAsync(Guid userId, Guid equipmentId, CancellationToken cancellationToken = default);
}
