using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MakerspaceFablabPlatform.Entitys.Enums;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Entitys;

[Table("Categories")]
[Index(nameof(Name), IsUnique = true)]
public class Category
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(20)] 
    public string Name { get; set; } =  string.Empty;
    
    [Required]
    public CategoryType Type { get; set;} = CategoryType.Undefined;
    
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}