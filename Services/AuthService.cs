using MarkerspaceFablabPlatform.Data;
using MarkerspaceFablabPlatform.Dtos.Auth;
using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Entitys.Enums;
using MarkerspaceFablabPlatform.Excepitons;
using MarkerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using MarkerspaceFablabPlatform.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValidationException = MarkerspaceFablabPlatform.Excepitons.ValidationException;


namespace MarkerspaceFablabPlatform.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IApplicationDbContext dbContext, ILogger<AuthService> logger, TokenService tokenService, IPasswordHasher<User> passwordHasher, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator)
    {
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _logger = logger;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }
    
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        bool emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        
        if (emailExists)
            throw new ConflictException("Email already exists");

        var user = new User()
        {
            Username = request.Username, // BUGFIX: username hiç atanmıyordu, boş kaydediliyordu
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Type = UserType.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
        };
        
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Registered user {UserId}", user.Id);

        return new AuthResponse()
        {
            Email = user.Email,
            Type = user.Type,
            Token = _tokenService.GenerateToken(user)
        };

    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        
        var validation  = await _loginValidator.ValidateAsync(request);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary()); 
                
        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (dbUser is null)
        {
            _logger.LogWarning("Login failed for {Email}: user not found", request.Email);
            throw new UnauthorizedException("Email or Password incorrect");
        }

        var result = _passwordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed for user {UserId}: wrong password", dbUser.Id);
            throw new UnauthorizedException("Email or Password incorrect");
        }

        if (!dbUser.IsActive || dbUser.IsDeleted)
        {
            _logger.LogWarning("Login blocked for user {UserId}: inactive or deleted", dbUser.Id);
            throw new UnauthorizedException("Email or Password incorrect");
        }
        

        _logger.LogInformation("User {UserId} logged in", dbUser.Id);

        return new AuthResponse()
        {
            Token = _tokenService.GenerateToken(dbUser),
            Email = dbUser.Email,
            Type = dbUser.Type
        };
    }
}