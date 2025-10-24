using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

/// <summary>
/// Repository for managing application settings
/// </summary>
public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly IDbContextFactory<QuranAppDbContext> _contextFactory;

    public AppSettingsRepository(IDbContextFactory<QuranAppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Get the application settings (single row, ID = 1)
    /// </summary>
    public async Task<AppSettings> GetSettingsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var settings = await context.AppSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            // Create default settings if none exist
            settings = new AppSettings
            {
                Id = 1,
                SchoolNameArabic = "مدرسة تحفيظ القرآن الكريم",
                SchoolLogoPath = null,
                ModifiedDate = DateTime.UtcNow
            };
            
            context.AppSettings.Add(settings);
            await context.SaveChangesAsync();
        }
        
        return settings;
    }

    /// <summary>
    /// Update application settings
    /// </summary>
    public async Task UpdateSettingsAsync(AppSettings settings)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        settings.ModifiedDate = DateTime.UtcNow;
        context.AppSettings.Update(settings);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Ensure default settings exist in database
    /// </summary>
    public async Task EnsureSettingsExistAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var exists = await context.AppSettings.AnyAsync();
        
        if (!exists)
        {
            var defaultSettings = new AppSettings
            {
                Id = 1,
                SchoolNameArabic = "مدرسة تحفيظ القرآن الكريم",
                SchoolLogoPath = null,
                ModifiedDate = DateTime.UtcNow
            };
            
            context.AppSettings.Add(defaultSettings);
            await context.SaveChangesAsync();
        }
    }
}
