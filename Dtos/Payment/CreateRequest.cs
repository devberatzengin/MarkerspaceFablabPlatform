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
    [Range(2, double.PositiveInfinity, ErrorMessage = "Amount must be greater than or equal to 2.")]
    public double RentalFee { get; set; }
    
    [Range(2, double.PositiveInfinity, ErrorMessage = "Amount must be greater than or equal to 2.")]
    public double LateFee { get; set; } = 0;
    
    [Required]
    [Range(2, double.PositiveInfinity, ErrorMessage = "Amount must be greater than or equal to 2.")]
    public double TotalAmount { get; set; }

    [Range(2, double.PositiveInfinity, ErrorMessage = "Amount must be greater than or equal to 2.")]
    public double DiscountAmount { get; set; } = 0;
    
    [Required]
    [Range(2, double.PositiveInfinity, ErrorMessage = "Amount must be greater than or equal to 2.")]
    public double PaidAmount { get; set; }

    [Required] 
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;
}