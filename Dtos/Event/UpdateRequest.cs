using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Event;

public class UpdateRequest
{
    [Required(ErrorMessage = "Event cant update without id")]
    public Guid Id { get; set; }
    
    public string Name { get; set; }  = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Location { get; set; } =  string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public Guid CategoryId { get; set; }
}