using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuranListeningApp.Infrastructure.Data.Seeders;

namespace QuranListeningApp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuranAppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuranAppDbContext>>();

        try
        {
            // Ensure database is created
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully.");

            // Seed Surah references if not already seeded
            if (!await context.SurahReferences.AnyAsync())
            {
                var surahs = SurahSeeder.GetSurahs();
                await context.SurahReferences.AddRangeAsync(surahs);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} Surah references.", surahs.Count);
            }
            else
            {
                logger.LogInformation("Surah references already seeded.");
            }

            // Seed default admin user if not already exists
            if (!await context.Users.AnyAsync(u => u.Username == "admin"))
            {
                var admin = DefaultUserSeeder.GetDefaultAdmin();
                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
                logger.LogInformation("Created default admin user (Username: admin, Password: Admin@123).");
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
