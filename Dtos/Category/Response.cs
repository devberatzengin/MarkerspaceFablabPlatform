using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Category;

public class Response
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }  = string.Empty;
    
    public CategoryType Type { get; set; }
    
    public bool IsActive { get; set; }
    
}