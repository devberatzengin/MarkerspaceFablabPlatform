using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Entitys.Enums;
using Microsoft.AspNetCore.Identity;

namespace MarkerspaceFablabPlatform.Data;

public static class SeedData
{
    public static void EnsureAdmin(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config)
    {
        if (db.Users.Any(u => u.Type == UserType.Admin))
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

        db.Users.Add(admin);
        db.SaveChanges();
    }
}
