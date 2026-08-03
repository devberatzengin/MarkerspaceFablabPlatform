using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public interface IStateFactory
{
    IEquipmentState Create(EquipmentStatus status);
}