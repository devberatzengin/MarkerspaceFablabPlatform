using System.Linq.Expressions;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace MakerspaceFablabPlatform.Data.CachedRepositoties;

public class CachedCategoryRepository : ICategoryRepository
{
    private readonly ICategoryRepository _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedCategoryRepository> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string AllCategoryKey = "category:all";
    private static string CategoryKey(Guid id) => $"category:{id}";

    private static CancellationTokenSource _resetCacheToken = new();

    private MemoryCacheEntryOptions BuildEntryOptions()
    {
        return new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));
    }

    private void InvalidateAll()
    {
        var previousToken = _resetCacheToken;
        _resetCacheToken = new CancellationTokenSource();

        if (!previousToken.IsCancellationRequested)
        {
            previousToken.Cancel();
        }
        previousToken.Dispose();
    }

    public CachedCategoryRepository(ICategoryRepository inner, IMemoryCache cache, ILogger<CachedCategoryRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public IQueryable<Category> Query(bool asNoTracking = true)
    {
        return _inner.Query(asNoTracking);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CategoryKey(id), out Category? cached))
        {
            _logger.LogInformation("[CACHE HIT] Category {Id} cache'ten geldi", id);
            return cached;
        }

        _logger.LogInformation("[CACHE MISS] Category {Id} veritabanından çekiliyor", id);
        var result = await _cache.GetOrCreateAsync(CategoryKey(id), async entry =>
        {
            entry.SetOptions(BuildEntryOptions());
            return await _inner.GetByIdAsync(id, cancellationToken);
        });

        return result;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(AllCategoryKey, out IReadOnlyList<Category>? cached))
        {
            _logger.LogInformation("[CACHE HIT] Tüm kategoriler cache'ten geldi");
            return cached!;
        }

        _logger.LogInformation("[CACHE MISS] Tüm kategoriler veritabanından çekiliyor");
        return await _cache.GetOrCreateAsync(AllCategoryKey, async entry =>
        {
            entry.SetOptions(BuildEntryOptions());
            return await _inner.GetAllAsync(cancellationToken);
        }) ?? new List<Category>();
    }

    public Task<bool> ExistsAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _inner.ExistsAsync(predicate, cancellationToken);
    }

    public async Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        var created = await _inner.AddAsync(entity, cancellationToken);
        InvalidateAll();
        return created;
    }

    public void Update(Category entity)
    {
        _inner.Update(entity);
        InvalidateAll();
    }

    public void Remove(Category entity)
    {
        _inner.Remove(entity);
        InvalidateAll();
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return _inner.NameExistsAsync(name, excludeId, cancellationToken);
    }
}
