using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;

namespace QuranListeningApp.Infrastructure.Data.Seeders;

public static class DefaultUserSeeder
{
    public static User GetDefaultAdmin()
    {
        // Create admin with default password: Admin@123
        // Password hash created using BCrypt or similar should be implemented properly in production
        var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        return new User
        {
            Id = adminId,
            Username = "admin",
            // This is a placeholder - proper password hashing should be implemented
            // For now using a simple hash representation
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FullNameArabic = "المسؤول",
            Email = "admin@qurann.local",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedByUserId = adminId // Self-created
        };
    }
}
