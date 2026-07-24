using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Event;

public enum EventPeriod
{
    Upcoming, 
    Past      
}

public class ListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public Guid? CategoryId { get; set; }

    public ContentStatus? Status { get; set; }

    public string? Search { get; set; }

    public DateTime? StartFrom { get; set; }
    public DateTime? StartTo { get; set; }

    public EventPeriod? Period { get; set; }
}
