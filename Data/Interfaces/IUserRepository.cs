using MakerspaceFablabPlatform.Entitys;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken = default);
}
