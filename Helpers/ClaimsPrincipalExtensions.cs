using System.Security.Claims;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.Helpers;


public static class ClaimsPrincipalExtensions
{
    private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.Claims.Any(c =>
            (c.Type == RoleClaimType || c.Type == ClaimTypes.Role)
            && c.Value == "Admin");
    }

    public static Guid GetCurrentUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedException("Token'da kullanıcı kimliği bulunamadı.");
        return Guid.Parse(value);
    }
}
