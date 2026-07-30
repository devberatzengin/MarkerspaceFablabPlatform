using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public class AvailableState : IEquipmentState
{
    public Task AvailableAsync(Equipment equipment, EquipmentRental rental, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("This equipment is already available available");
    }

    public async Task ReservedAsync(Equipment equipment, EquipmentRental rental, IUnitOfWork unitOfWork)
    {
        equipment.Status = EquipmentStatus.Reserved;
        equipment.UpdatedAt  = DateTime.UtcNow;
        unitOfWork.Equipments.Update(equipment);
        
        await unitOfWork.EquipmentRentals.AddAsync(rental);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RentedAsync(Equipment equipment, EquipmentRental rental, IUnitOfWork unitOfWork)
    {
        equipment.Status = EquipmentStatus.Rented;
        equipment.UpdatedAt  = DateTime.UtcNow;
        unitOfWork.Equipments.Update(equipment);
        
        await unitOfWork.EquipmentRentals.AddAsync(rental);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task MaintenanceAsync(Equipment equipment, IUnitOfWork unitOfWork)
    {
        equipment.Status = EquipmentStatus.Maintenance;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
        unitOfWork.Equipments.Update(equipment);
        await unitOfWork.Equipments.SaveChangesAsync();
    }
}
