using MarkerspaceFablabPlatform.Dtos.Common;
using MarkerspaceFablabPlatform.Dtos.Event;

namespace MarkerspaceFablabPlatform.Services.Interfaces;

public interface IEventService
{
    Task<Response> CreateAsync(CreateRequest createRequest, Guid currentUserId);
    Task<PagedResponse<Response>> GetAllAsync(ListRequest request, bool isAdmin);
    Task<Response?> GetByIdAsync(Guid eventId, bool isAdmin);
    Task<Response?> UpdateAsync(UpdateRequest request, Guid currentUserId);
    Task<Response?> PublishAsync(Guid eventId, Guid currentUserId);
    Task<Response?> UnpublishAsync(Guid eventId, Guid currentUserId);
    Task<bool> ArchiveAsync(Guid eventId, Guid currentUserId);
}
