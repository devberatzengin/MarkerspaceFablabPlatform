namespace MakerspaceFablabPlatform.Dtos.Category;

using MakerspaceFablabPlatform.Entitys.Enums;
using System.ComponentModel.DataAnnotations;

public class UpdateRequest
{
    public Guid Id { get; set; }
    
    [Required, MinLength(3), MaxLength(20)]
    public string Name { get; set; } = string.Empty;
    
    public CategoryType Type { get; set;} = CategoryType.Undefined;
    
    public bool IsActive { get; set; } = true;
    
}