using System.ComponentModel.DataAnnotations;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Payment;

public class CreateRequest
{
    [Required]
    public Guid EquipmentRentalId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public decimal RentalFee { get; set; }
    
    
    public decimal LateFee { get; set; } = 0;
    
    [Required]
    public decimal TotalAmount { get; set; }


    public decimal DiscountAmount { get; set; } = 0;
    
    [Required]
    public decimal PaidAmount { get; set; }

    [Required] 
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;
}