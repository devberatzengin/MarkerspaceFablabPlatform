using System.ComponentModel.DataAnnotations;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Equipment;

public class UpdateRequest
{
    [Required]
    public Guid Id { get; set; }
    
    [MaxLength(20)] 
    public string? Name { get; set; } =  string.Empty;
    
    // eğer ben buraya defult değer koyarasm ve istek atarken bu kısma değer vermez isem bu default değer db hye işlenir mo onu denicem bi ara
    public EquipmentStatus? Status { get; set; } = EquipmentStatus.Unknown;

    public TimeSpan? UsageTime { get; set; } = TimeSpan.Zero;
    
    [Range(minimum:1, maximum:10,ErrorMessage = "Equipment hard level must be between 1 and 10")]
    public short? RequiredUserLevel { get; set; } =  1;
    
}