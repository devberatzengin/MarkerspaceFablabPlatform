using MakerspaceFablabPlatform.Dtos.Subscription;

namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface ISubscriptionService
{
    Task<Response> CreateAsync(CreateRequest request, Guid currentUserId, CancellationToken token = default);
    Task<bool> DeleteAsync(Guid subscriptionId, Guid currentUserId, CancellationToken token = default);
    Task<List<Response>> GetMineAsync(Guid currentUserId, CancellationToken token = default);
}
