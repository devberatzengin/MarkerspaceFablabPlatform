using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface ITokenService
{
    public string GenerateToken(User user);

}