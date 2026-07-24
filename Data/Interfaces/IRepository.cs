using MarkerspaceFablabPlatform.Dtos.Common;

namespace MarkerspaceFablabPlatform.Data.Interfaces;

public interface IRepository <T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<PagedResponse<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    
    Task<T> DeleteAsync(Guid id);
}