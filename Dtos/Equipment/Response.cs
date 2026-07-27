using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Equipment;

public class Response
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } =  string.Empty;

    
    public EquipmentType Type { get; set; } = EquipmentType.Unknown;

    public EquipmentPlacementType PlacementType { get; set; } = EquipmentPlacementType.Unknown;

    public EquipmentStatus Status { get; set; } = EquipmentStatus.Unknown;
        
    public short RequiredUserLevel { get; set; } =  1;
    
    
    public bool IsDeleted { get; set; } = false;

    public Guid? UsingById { get; set; } = Guid.Empty;

    public DateTime AvailableAt  { get; set; } = DateTime.UtcNow;
}