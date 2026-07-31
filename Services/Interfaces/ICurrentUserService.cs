namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    bool IsAdmin();
}