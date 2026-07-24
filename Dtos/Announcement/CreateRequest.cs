using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Announcement;

using MakerspaceFablabPlatform.Entities.Enums;

public class CreateRequest
{
    [Required, MinLength(5), MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)] 
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public Guid CategoryId { get; set; }
}