using MarkerspaceFablabPlatform.Dtos.User;

namespace MarkerspaceFablabPlatform.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();                          // Admin
    Task<UserResponse> GetByIdAsync(Guid id);
    Task<UserResponse> UpdateAsync(Guid id, UpdateRequest request, Guid currentUserId);
    Task DeactivateAsync(Guid id); 
    Task ActivateAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequest request);
}