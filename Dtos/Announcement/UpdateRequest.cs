using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Announcement;

public class UpdateRequest
{
    [Required]
    public Guid Id { get; set;}
    
    [Required, MinLength(5), MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)] 
    public string Content { get; set; } = string.Empty;
    
    public Guid CategoryId { get; set; }
}