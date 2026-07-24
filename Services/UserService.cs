using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.User;
using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;

namespace MakerspaceFablabPlatform.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IValidator<UpdateRequest> _validator;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger, IValidator<UpdateRequest> validator, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _logger = logger;
        _validator = validator;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<List<UserResponse>> GetAllAsync()
    {
        return await _userRepository.Query()
            .Select(u => new UserResponse
            {
                Id = u.Id,
                UserName =  u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                Type = u.Type,
                IsActive = u.IsActive,
                CreatedAt =  u.CreatedAt
            })
            .ToListAsync();
    }


    public async Task<UserResponse> GetByIdAsync(Guid id) // Admin Method
    {
        var result =  await _userRepository.GetByIdAsync(id);
        if (result is null)
            throw new NotFoundException(nameof(User), id);

        return new UserResponse()
        {
            Id = result.Id,
            Email = result.Email,
            UserName = result.Username,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Type = result.Type,
            PhoneNumber = result.PhoneNumber,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateRequest request, Guid currentUserId)
    {

        if (id != currentUserId)
        {
            throw new ForbiddenException("Sen Başka Birisini güncellemeye çalışıyorsun");
        }
        
        var validation = await _validator.ValidateAsync(request);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        var dbUser = await _userRepository.GetByIdAsync(id);
        if (dbUser is null)
            throw new NotFoundException(nameof(User), id);
        
        dbUser.FirstName = request.FirstName;
        dbUser.LastName = request.LastName;
        dbUser.PhoneNumber = request.PhoneNumber;
        dbUser.Email = request.Email;

        _userRepository.Update(dbUser);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", dbUser.Id);

        return new UserResponse()
        {
            Id = dbUser.Id,
            Email = dbUser.Email,
            UserName = dbUser.Username,

            FirstName = dbUser.FirstName,
            LastName = dbUser.LastName,
            PhoneNumber = dbUser.PhoneNumber,

            Type = dbUser.Type,
            IsActive = dbUser.IsActive,
            CreatedAt = dbUser.CreatedAt
        };

    }

    public async Task DeactivateAsync(Guid id) // Admin Endpoind yine
    {
         var dbUser = await _userRepository.GetByIdAsync(id);
         if (dbUser is null)
             throw new NotFoundException(nameof(User), id);
         
         dbUser.IsActive = false;
         await _userRepository.SaveChangesAsync();

         _logger.LogInformation("Deactivated user {UserId}", dbUser.Id);
    }

    public async Task ActivateAsync(Guid id) 
    {
         var dbUser = await _userRepository.GetByIdAsync(id);
         if (dbUser is null)
             throw new NotFoundException(nameof(User), id);

         dbUser.IsActive = true;
         await _userRepository.SaveChangesAsync();

         _logger.LogInformation("Activated user {UserId}", dbUser.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var dbUser = await _userRepository.GetByIdAsync(id);
        
        if (dbUser is null)
            throw new NotFoundException(nameof(User), id);
        
        dbUser.IsDeleted = true;
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted user {UserId}", dbUser.Id);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var dbUser= await _userRepository.GetByIdAsync(id);
        
        if (dbUser is null)
            throw new NotFoundException(nameof(User), id);
        
        var result = _passwordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, request.CurrentPassword);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new ConflictException("Mevcut şifre hatalı.");
        }
        
        dbUser.PasswordHash = _passwordHasher.HashPassword(dbUser, request.NewPassword);
        dbUser.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.SaveChangesAsync();
        
        _logger.LogInformation("Password changed {UserId}", dbUser.Id);
        
    }
    
}