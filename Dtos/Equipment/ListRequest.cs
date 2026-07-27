using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Equipment;

public class ListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public EquipmentStatus? Status { get; set; }
    public EquipmentType? Type { get; set; }
    public EquipmentPlacementType? PlacementType { get; set; }

    public string? Search { get; set; }
}
