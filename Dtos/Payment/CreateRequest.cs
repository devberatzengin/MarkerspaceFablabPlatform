using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Payment;

public class CreateRequest
{
    [Required]
    public Guid EquipmentRentalId { get; set; }
    
    public Guid? UserId { get; set; }
}
