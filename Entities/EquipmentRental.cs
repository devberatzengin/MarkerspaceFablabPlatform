using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerspaceFablabPlatform.Entities;

[Table("EquipmentRentals")]
public class EquipmentRental
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    [Required]
    public DateTime RentedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReleasedAt { get; set; }

    [Required]
    public DateTime ExpectedReturnAt { get; set; }

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    
    public bool IsOverdue { get; set; } = false;
    public TimeSpan? OverdueBy { get; set; }

}
