using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public class StateFactory : IStateFactory
{
    public IEquipmentState Create(EquipmentStatus status)
    {
        return status switch
        {
            _ when status == EquipmentStatus.Available => new AvailableState(),
            _ when status == EquipmentStatus.Maintenance => new MaintenanceState(),
            _ when status == EquipmentStatus.Rented => new RentedState(),
            _ when status == EquipmentStatus.Reserved => new ReservedState(),
            _ => throw new NotSupportedException($"Equipment status {status} is not supported.")
        };
    }
}