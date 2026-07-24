using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Entities;

[Table("Equipments")]
public class Equipment
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(20)] 
    public string Name { get; set; } =  string.Empty;
    
    [MaxLength(100)]
    public string Description { get; set; } =  string.Empty;

    [Required] 
    public EquipmentType Type { get; set; } = EquipmentType.Unknown;

    [Required]
    public EquipmentPlacementType PlacementType { get; set; } = EquipmentPlacementType.Unknown;

    [Required] 
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Unknown;
        
    [Required]
    [Range(minimum:1, maximum:10,ErrorMessage = "Equipment hard level must be between 1 and 10")]
    public short EquipmentHardLevel { get; set; } =  1;
    
    [Required]
    public bool IsDeleted { get; set; } = false;
    
    
    
    [Required]
    public Guid UsingById { get; set; } 
    
    public User UsingBy { get; set; } = null!;
    
    
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime AvailableAt  { get; set; } = DateTime.UtcNow;
}