using System.Security.Claims;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services;
using MakerspaceFablabPlatform.Services.Interfaces;

namespace MakerspaceFablabPlatform.Services;

public class CurrentUserService : ICurrentUserService
{
    private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        var value = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedException("Token'da kullanıcı kimliği bulunamadı.");

        if (!Guid.TryParse(value, out var userId) || userId == Guid.Empty)
        {
            throw new UnauthorizedException("Geçersiz kullanıcı kimliği.");
        }

        return userId;
    }
    

    public bool IsAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        return user.Claims.Any(c =>
            (c.Type == RoleClaimType || c.Type == ClaimTypes.Role) && c.Value == "Admin");
    }
}