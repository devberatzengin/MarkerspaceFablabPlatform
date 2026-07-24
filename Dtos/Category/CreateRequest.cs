using System.ComponentModel.DataAnnotations;

namespace MarkerspaceFablabPlatform.Dtos.Category;


using MarkerspaceFablabPlatform.Entitys.Enums;

public class CreateRequest
{
    [Required, MinLength(3), MaxLength(20)]
    public string Name { get; set; } =  string.Empty;
    
    [Required]
    public CategoryType Type { get; set;} = CategoryType.Undefined;
}