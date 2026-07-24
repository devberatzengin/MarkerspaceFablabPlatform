using MarkerspaceFablabPlatform.Dtos.Auth;

namespace MarkerspaceFablabPlatform.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}