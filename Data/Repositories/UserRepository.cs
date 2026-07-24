using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Entitys.Enums;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        Query().AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken = default) =>
        Query().AnyAsync(u => u.Type == UserType.Admin, cancellationToken);
}
