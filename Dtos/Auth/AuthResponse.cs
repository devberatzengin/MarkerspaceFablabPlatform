using MarkerspaceFablabPlatform.Entitys.Enums;

namespace MarkerspaceFablabPlatform.Dtos.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserType Type { get; set; } = UserType.User;    
}