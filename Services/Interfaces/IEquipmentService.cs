using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Equipment;
using RentalResponse = MakerspaceFablabPlatform.Dtos.EquipmentRental;
using MakerspaceFablabPlatform.Entities.Enums;
using EquipmentRentalResponse = MakerspaceFablabPlatform.Dtos.EquipmentRental.Response;

namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface IEquipmentService
{
    Task<Response?> GetByIdAsync(Guid id, CancellationToken token);
    Task<PagedResponse<Response>> GetAllAsync(ListRequest request, CancellationToken token);
    Task<Response> CreateAsync(CreateRequest request, Guid currentUserId ,CancellationToken token);
    Task<Response> UpdateAsync(UpdateRequest request, Guid currentUserId, CancellationToken token);
    Task<Response> DeleteAsync(Guid id, CancellationToken token);
    
    Task<Response> RentAsync(Guid id,TimeSpan span, Guid currentUserId, CancellationToken token);
    Task<Response> ReserveAsync(Guid id,TimeSpan span, Guid currentUserId, CancellationToken token);
    
    Task<Response> ReleaseItAsync(Guid id, Guid currentUserId, CancellationToken token);
    Task<Response> SetMaintenanceAsync(Guid id, CancellationToken token);
    
    Task<PagedResponse<EquipmentRentalResponse>> MyEquipmentsAsync(Guid userId, ListRequest request,bool includePast, CancellationToken token);
}