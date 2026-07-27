using System.ComponentModel.DataAnnotations;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Equipment;

public class CreateRequest
{
    [Required]
    [MaxLength(20)]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Description { get; set; } =  string.Empty;
    
    [Required] 
    public EquipmentType Type { get; set; } = EquipmentType.Unknown;

    [Required]
    public EquipmentPlacementType PlacementType { get; set; } = EquipmentPlacementType.Unknown;

    //[Required] 
    //public EquipmentStatus Status { get; set; } = EquipmentStatus.Unknown;
    //Yeni ekiplanın girişini available diye düşünüyoruz sadece o yüzden kapattım
    
    //Yani bu equipment'i min kaç level olan birisi kullanabilir
    [Required]
    [Range(minimum:1, maximum:10,ErrorMessage = "Equipment hard level must be between 1 and 10")]
    public short RequiredUserLevel { get; set; } =  1;
}