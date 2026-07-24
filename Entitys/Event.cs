namespace MarkerspaceFablabPlatform.Entitys;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MarkerspaceFablabPlatform.Entitys.Enums;


[Table("Event")]
public class Event
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }  = string.Empty;
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    [Required]
    public DateTime EndDate { get; set; } = DateTime.Today;
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    public Category Category { get; set; } = null!;

    
    public Guid CreatedByUserId { get; set; }
    
    public User CreatedBy { get; set; } = null!;

    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}