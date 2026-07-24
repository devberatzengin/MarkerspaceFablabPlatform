using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Entities;


[Table("Announcement")]
public class Announcement
{

    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required] 
    public Guid Id { get; set; }  = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)] 
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)] 
    public String Content { get; set; } = string.Empty;
    
    
    
    [Required]
    public Guid CreatedByUserId { get; set; } 
    public User CreatedBy { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    
    [Column("status")]
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}   