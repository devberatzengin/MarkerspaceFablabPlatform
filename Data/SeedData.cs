using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Entitys.Enums;
using Microsoft.AspNetCore.Identity;

namespace MakerspaceFablabPlatform.Data;

public static class SeedData
{
    public static async Task EnsureAdminAsync(IUserRepository userRepository, IPasswordHasher<User> hasher, IConfiguration config)
    {
        if (await userRepository.AnyAdminExistsAsync())
            return;

        var email = config["SeedAdmin:Email"] ?? "admin@example.com";
        var password = config["SeedAdmin:Password"] ?? "Admin123!";

        var admin = new User
        {
            Username = "admin",
            Email = email,
            FirstName = "System",
            LastName = "Admin",
            Type = UserType.Admin,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, password);

        await userRepository.AddAsync(admin);
        await userRepository.SaveChangesAsync();
    }
}
