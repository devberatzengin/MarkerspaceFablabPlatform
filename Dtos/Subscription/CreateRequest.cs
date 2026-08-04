using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Subscription;

public class CreateRequest
{
    public SubscriptionTargetType TargetType { get; set; } = SubscriptionTargetType.Category;
    public Guid? CategoryId { get; set; }
    public Guid? EquipmentId { get; set; }
    public NotificationChannelType Channel { get; set; } = NotificationChannelType.InApp;
}
