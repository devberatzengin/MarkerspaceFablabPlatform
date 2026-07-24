using MakerspaceFablabPlatform.Entitys;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
