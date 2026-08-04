using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public class AvailableState : IEquipmentState
{
    public Task AvailableAsync(Equipment equipment, EquipmentRental rental)
    {
        throw new ConflictException("This equipment is already available available");
    }

    public Task ReservedAsync(Equipment equipment, EquipmentRental rental)
    {
        equipment.Status = EquipmentStatus.Reserved;
        equipment.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task RentedAsync(Equipment equipment, EquipmentRental rental)
    {
        equipment.Status = EquipmentStatus.Rented;
        equipment.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public async Task MaintenanceAsync(Equipment equipment)
    {
        equipment.Status = EquipmentStatus.Maintenance;
        equipment.UpdatedAt  = DateTime.UtcNow;
    }
}
