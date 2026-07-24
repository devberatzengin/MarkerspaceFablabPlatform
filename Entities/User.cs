using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Entities;

[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } =  Guid.NewGuid();

    [Required] 
    public UserType Type { get; set; } = UserType.Unknown;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)] 
    public string FirstName { get; set; } = string.Empty;
    
    [MaxLength(20)] 
    public string LastName { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}