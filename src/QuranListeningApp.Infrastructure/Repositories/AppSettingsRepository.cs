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
    private readonly QuranAppDbContext _context;

    public AppSettingsRepository(QuranAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get the application settings (single row, ID = 1)
    /// </summary>
    public async Task<AppSettings> GetSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        
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
            
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        return settings;
    }

    /// <summary>
    /// Update application settings
    /// </summary>
    public async Task UpdateSettingsAsync(AppSettings settings)
    {
        settings.ModifiedDate = DateTime.UtcNow;
        _context.AppSettings.Update(settings);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Ensure default settings exist in database
    /// </summary>
    public async Task EnsureSettingsExistAsync()
    {
        var exists = await _context.AppSettings.AnyAsync();
        
        if (!exists)
        {
            var defaultSettings = new AppSettings
            {
                Id = 1,
                SchoolNameArabic = "مدرسة تحفيظ القرآن الكريم",
                SchoolLogoPath = null,
                ModifiedDate = DateTime.UtcNow
            };
            
            _context.AppSettings.Add(defaultSettings);
            await _context.SaveChangesAsync();
        }
    }
}
