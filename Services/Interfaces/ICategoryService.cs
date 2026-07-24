using MakerspaceFablabPlatform.Dtos.Category;

namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface ICategoryService
{
     Task<Response> CreateAsync(CreateRequest createRequest);
     Task<List<Response>> GetAllAsync(bool includeUnactivated = false);
     Task<Response?> GetByIdAsync(Guid categoryId,bool includeUnactivated = false);
     Task<Response?> UpdateAsync(UpdateRequest updateRequest);
     Task<Response?> DeactivateAsync(Guid categoryId); 
     Task<bool> DeleteAsync(Guid categoryId);
}