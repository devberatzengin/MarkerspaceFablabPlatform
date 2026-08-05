using System.ComponentModel.DataAnnotations;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Entities;

public class Payment
{
    [Key]
    [Required]
    public Guid Id { get; set; } =  Guid.NewGuid();
    [Required]
    public string PaymentNumber { get; set; } = string.Empty;
    
    
    [Required]
    public Guid EquipmentRentalId { get; set; }

    public EquipmentRental EquipmentRental { get; set; } = null!;
    
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    
    public decimal RentalFee { get; set; } // Base rental fee
    public decimal LateFee { get; set; }   // 0 if on-time, > 0 if late
    public decimal TotalAmount { get; set; } // RentalFee + LateFee - Discount
    public decimal DiscountAmount { get; set; } // Membership discount

    [Required] public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;// Cash/Card/Transfer/Other

    [Required] public PaymentStatus Status { get; set; } = PaymentStatus.Pending;// Pending/Paid/Failed/Refunded
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public bool IsDeleted { get; set; }

}