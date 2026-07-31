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
    public decimal RentalFee { get; set; }
    
    [Required]
    public decimal LateFee { get; set; }   // geçiktiysen üstüne fiyat farkı alıyoruz
    
    [Required]
    public decimal TotalAmount { get; set; } // RentalFee + LateFee - Discount
    
    [Required]
    public decimal DiscountAmount { get; set; } // Membership seviyene göre discount oluyor üyelere
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;

    [Required]
    public PaymentStatus Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime? PaidAt { get; set; }
}