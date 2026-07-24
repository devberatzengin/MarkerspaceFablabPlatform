using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Event;

public class Response
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public Guid CategoryId { get; set; }
    
    
    public ContentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}