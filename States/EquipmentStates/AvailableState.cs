using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public class AvailableState : IEquipmentState
{
    public Task AvailableAsync(Equipment equipment, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("This equipment is already available available");
    }

    public async Task ReservedAsync(Equipment equipment, IUnitOfWork unitOfWork)
    {
        equipment.Status = EquipmentStatus.Reserved;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
        unitOfWork.Equipments.Update(equipment);
        await unitOfWork.Equipments.SaveChangesAsync();
    }

    public async Task RentedAsync(Equipment equipment, IUnitOfWork unitOfWork)
    {
        
        equipment.Status = EquipmentStatus.Rented;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
        unitOfWork.Equipments.Update(equipment);
        await unitOfWork.Equipments.SaveChangesAsync();
    }

    public async Task MaintenanceAsync(Equipment equipment, IUnitOfWork unitOfWork)
    {
        equipment.Status = EquipmentStatus.Maintenance;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
        unitOfWork.Equipments.Update(equipment);
        await unitOfWork.Equipments.SaveChangesAsync();
    }
}
