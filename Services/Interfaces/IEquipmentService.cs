using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Equipment;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface IEquipmentService
{
    Task<Response?> GetByIdAsync(Guid id, CancellationToken token);
    Task<PagedResponse<Response>> GetAllAsync(CancellationToken token);
    Task<Response> CreateAsync(CreateRequest request, Guid currentUserId ,CancellationToken token);
    Task<Response> UpdateAsync(UpdateRequest request, Guid currentUserId, CancellationToken token);
    Task<Response> DeleteAsync(Guid id, CancellationToken token);
    
    Task<Response> RentAsync(Guid id,TimeSpan span, Guid currentUserId, CancellationToken token);
    Task<Response> ReserveAsync(Guid id,TimeSpan span, Guid currentUserId, CancellationToken token);
    
    Task<Response> ReleaseItAsync(Guid id, Guid currentUserId, CancellationToken token);
    
}