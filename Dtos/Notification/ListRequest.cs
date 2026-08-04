namespace MakerspaceFablabPlatform.Dtos.Notification;

public class ListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public bool OnlyUnread { get; set; } = false;
}
