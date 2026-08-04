using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Notification;

namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface INotificationService
{
    Task<PagedResponse<Response>> GetMineAsync(Guid currentUserId, ListRequest request, CancellationToken token = default);
    Task<Response> MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken token = default);
    Task<int> GetUnreadCountAsync(Guid currentUserId, CancellationToken token = default);
}
