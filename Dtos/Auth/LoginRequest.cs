using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Auth;

public class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}