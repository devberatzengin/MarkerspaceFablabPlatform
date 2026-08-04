using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Notifications;

public class NotificationMessage
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public int? ThresholdMinutes { get; set; }
}