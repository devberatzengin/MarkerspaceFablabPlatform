using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)] 
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(20)] 
    public string LastName { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
}