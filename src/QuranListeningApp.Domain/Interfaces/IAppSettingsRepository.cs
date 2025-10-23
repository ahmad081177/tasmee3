using QuranListeningApp.Domain.Entities;

namespace QuranListeningApp.Domain.Interfaces;

/// <summary>
/// Repository interface for application settings
/// </summary>
public interface IAppSettingsRepository
{
    /// <summary>
    /// Get the application settings (single row)
    /// </summary>
    Task<AppSettings> GetSettingsAsync();

    /// <summary>
    /// Update application settings
    /// </summary>
    Task UpdateSettingsAsync(AppSettings settings);

    /// <summary>
    /// Initialize default settings if none exist
    /// </summary>
    Task EnsureSettingsExistAsync();
}
