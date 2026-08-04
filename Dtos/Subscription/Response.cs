using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Subscription;

public class Response
{
    public Guid Id { get; set; }
    public SubscriptionTargetType TargetType { get; set; }

    public Guid? CategoryId { get; set; } 
    public string? CategoryName { get; set; }

    
    public Guid? EquipmentId { get; set; }
    public string? EquipmentName { get; set; }

    
    public NotificationChannelType Channel { get; set; }

    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
