using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Category;


using MakerspaceFablabPlatform.Entitys.Enums;

public class CreateRequest
{
    [Required, MinLength(3), MaxLength(20)]
    public string Name { get; set; } =  string.Empty;
    
    [Required]
    public CategoryType Type { get; set;} = CategoryType.Undefined;
}