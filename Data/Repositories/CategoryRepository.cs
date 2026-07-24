using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entitys;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        Query().AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), cancellationToken);
}
