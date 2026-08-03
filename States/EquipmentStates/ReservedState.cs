using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public class ReservedState : IEquipmentState
{
    public async Task AvailableAsync(Equipment equipment, EquipmentRental rental)
    {
        rental.ReleasedAt = DateTime.UtcNow;
        equipment.Status = EquipmentStatus.Available;
        equipment.UpdatedAt  = DateTime.UtcNow;
    }

    public Task ReservedAsync(Equipment equipment, EquipmentRental rental)
    {
        throw new ConflictException("The equipment is already reserved.");
    }

    public Task RentedAsync(Equipment equipment,EquipmentRental rental)
    {
        throw new ConflictException("The equipment have to be release before get rented.");
    }

    public async Task MaintenanceAsync(Equipment equipment)
    {
        equipment.Status = EquipmentStatus.Maintenance;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
    }
}