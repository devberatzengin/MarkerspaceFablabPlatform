using System.Linq.Expressions;
using MakerspaceFablabPlatform.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly IApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(IApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public IQueryable<T> Query(bool asNoTracking = true) =>
        asNoTracking ? DbSet.AsNoTracking() : DbSet;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync(new object?[] { id }, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(predicate, cancellationToken);

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        DbContext.SaveChangesAsync(cancellationToken);
}
