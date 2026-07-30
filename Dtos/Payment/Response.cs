using System.ComponentModel.DataAnnotations;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Payment;

public class Response
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public string PaymentNumber { get; set; } = string.Empty;

    [Required]
    public Guid EquipmentRentalId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public decimal RentalFee { get; set; } // Base rental fee
    
    [Required]
    public decimal LateFee { get; set; }   // 0 if on-time, > 0 if late
    
    [Required]
    public decimal TotalAmount { get; set; } // RentalFee + LateFee - Discount
    
    [Required]
    public decimal DiscountAmount { get; set; } // Membership discount
    
    [Required]
    public decimal PaidAmount { get; set; } // Actually paid
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;

    [Required]
    public PaymentStatus Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime? PaidAt { get; set; }
}