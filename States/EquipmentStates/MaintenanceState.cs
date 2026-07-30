using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public class MaintenanceState : IEquipmentState
{
    public async Task AvailableAsync(Equipment equipment,EquipmentRental rental, IUnitOfWork unitOfWork)
    {
        equipment.Status = EquipmentStatus.Available;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
        unitOfWork.Equipments.Update(equipment);
        await unitOfWork.Equipments.SaveChangesAsync();
    }

    public Task ReservedAsync(Equipment equipment, EquipmentRental rental, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("The equipment have to be available before get reserved.");
    }

    public Task RentedAsync(Equipment equipment,EquipmentRental rental, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("The equipment have to be available before get rented.");
    }

    public Task MaintenanceAsync(Equipment equipment, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("The equipment is already maintained.");
    }
}