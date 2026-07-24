using System.ComponentModel.DataAnnotations;

namespace MarkerspaceFablabPlatform.Dtos.User;

public class UpdateRequest
{
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
}