using System.Linq.Expressions;
using MakerspaceFablabPlatform.Dtos.Common;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query(bool asNoTracking = true);

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
