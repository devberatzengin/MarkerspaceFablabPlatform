using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Announcement;

public class ListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public Guid? CategoryId { get; set; }
    
    public ContentStatus? Status { get; set; }

    // keyword search
    public string? Search { get; set; }
    
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
