using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Entitys.Enums;

namespace MarkerspaceFablabPlatform.Dtos.Category;

public class Response
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }  = string.Empty;
    
    public CategoryType Type { get; set; }
    
    public bool IsActive { get; set; }
    
}