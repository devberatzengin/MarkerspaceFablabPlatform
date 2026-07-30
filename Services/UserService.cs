using AutoMapper;
using AutoMapper.QueryableExtensions;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.User;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;

namespace MakerspaceFablabPlatform.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository; // Kullanmıyorum ama ne olur ne olmaz silmek istemiyorum
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateRequest> _validator;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, ILogger<UserService> logger, IMapper mapper, IValidator<UpdateRequest> validator, IPasswordHasher<User> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<List<UserResponse>> GetAllAsync()
    {
        return await _unitOfWork.Users.Query()
            .ProjectTo<UserResponse>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }


    public async Task<UserResponse> GetByIdAsync(Guid id) // Admin Method
    {
        var result =  await _unitOfWork.Users.GetByIdAsync(id);
        if (result is null)
            throw new NotFoundException(nameof(User), id);

        return _mapper.Map<UserResponse>(result);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateRequest request, Guid currentUserId)
    {

        if (id != currentUserId)
        {
            throw new NotResourceOwnerException("Sen Başka Birisini güncellemeye çalışıyorsun");
        }
        
        var validation = await _validator.ValidateAsync(request);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        var dbUser = await _unitOfWork.Users.GetByIdAsync(id);
        if (dbUser is null)
            throw new NotFoundException(nameof(User), id);
        
        dbUser.FirstName = request.FirstName ?? dbUser.FirstName;
        dbUser.LastName = request.LastName ?? dbUser.LastName;
        dbUser.PhoneNumber = request.PhoneNumber ?? dbUser.PhoneNumber;
        dbUser.Email = request.Email ?? dbUser.Email;

        _unitOfWork.Users.Update(dbUser);
        
        await _unitOfWork.Users.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", dbUser.Id);

        return _mapper.Map<UserResponse>(dbUser);

    }

    public async Task DeactivateAsync(Guid id) // Admin Endpoind yine
    {
         var dbUser = await _unitOfWork.Users.GetByIdAsync(id);
         if (dbUser is null)
             throw new NotFoundException(nameof(User), id);
         
         dbUser.IsActive = false;
         await _unitOfWork.Users.SaveChangesAsync();

         _logger.LogInformation("Deactivated user {UserId}", dbUser.Id);
    }

    public async Task ActivateAsync(Guid id) 
    {
         var dbUser = await _unitOfWork.Users.GetByIdAsync(id);
         if (dbUser is null)
             throw new NotFoundException(nameof(User), id);

         dbUser.IsActive = true;
         await _unitOfWork.Users.SaveChangesAsync();

         _logger.LogInformation("Activated user {UserId}", dbUser.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var dbUser = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (dbUser is null)
            throw new NotFoundException(nameof(User), id);
        
        dbUser.IsDeleted = true;
        await _unitOfWork.Users.SaveChangesAsync();

        _logger.LogInformation("Deleted user {UserId}", dbUser.Id);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var dbUser= await _unitOfWork.Users.GetByIdAsync(id);
        
        if (dbUser is null)
            throw new NotFoundException(nameof(User), id);
        
        var result = _passwordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, request.CurrentPassword);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new InvalidCurrentPasswordException();
        }
        
        dbUser.PasswordHash = _passwordHasher.HashPassword(dbUser, request.NewPassword);
        dbUser.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Users.SaveChangesAsync();
        
        _logger.LogInformation("Password changed {UserId}", dbUser.Id);
        
    }

    public async Task<UserResponse> AddBalanceAsync(Guid userId, decimal balance)
    {
        var result = await _unitOfWork.Users.GetByIdAsync(userId);

        if (result is null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (balance <= 0)
            throw new ValidationException($"{balance}, cant be negative or zero");
        
        result.Balance += balance;
        _unitOfWork.Users.Update(result);
        await _unitOfWork.Users.SaveChangesAsync();
        
        return _mapper.Map<UserResponse>(result);
    }
    
}