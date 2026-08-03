using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForRelatedEntityAsync(Guid relatedEntityId, NotificationType type, int? thresholdMinutes = null, CancellationToken cancellationToken = default);
}
