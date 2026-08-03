using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(IApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Query().CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task<Notification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query(asNoTracking: false)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsForRelatedEntityAsync(Guid relatedEntityId, NotificationType type, int? thresholdMinutes = null, CancellationToken cancellationToken = default)
    {
        return Query().AnyAsync(n =>
            n.RelatedEntityId == relatedEntityId &&
            n.Type == type &&
            n.ThresholdMinutes == thresholdMinutes, cancellationToken);
    }
}
