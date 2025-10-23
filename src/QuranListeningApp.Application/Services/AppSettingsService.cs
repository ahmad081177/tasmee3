using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Interfaces;

namespace QuranListeningApp.Application.Services;

/// <summary>
/// Service for managing application settings
/// </summary>
public class AppSettingsService
{
    private readonly IAppSettingsRepository _appSettingsRepository;

    public AppSettingsService(IAppSettingsRepository appSettingsRepository)
    {
        _appSettingsRepository = appSettingsRepository;
    }

    /// <summary>
    /// Get current application settings
    /// </summary>
    public async Task<AppSettings> GetSettingsAsync()
    {
        return await _appSettingsRepository.GetSettingsAsync();
    }

    /// <summary>
    /// Update school name
    /// </summary>
    public async Task UpdateSchoolNameAsync(string schoolName, Guid modifiedByUserId)
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        settings.SchoolNameArabic = schoolName;
        settings.ModifiedByUserId = modifiedByUserId;
        await _appSettingsRepository.UpdateSettingsAsync(settings);
    }

    /// <summary>
    /// Update school logo path
    /// </summary>
    public async Task UpdateLogoPathAsync(string? logoPath, Guid modifiedByUserId)
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        settings.SchoolLogoPath = logoPath;
        settings.ModifiedByUserId = modifiedByUserId;
        await _appSettingsRepository.UpdateSettingsAsync(settings);
    }

    /// <summary>
    /// Get school logo path (or null if not set)
    /// </summary>
    public async Task<string?> GetLogoPathAsync()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return settings.SchoolLogoPath;
    }

    /// <summary>
    /// Get school name
    /// </summary>
    public async Task<string> GetSchoolNameAsync()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return settings.SchoolNameArabic;
    }

    /// <summary>
    /// Get pledge text (or default if not set)
    /// </summary>
    public async Task<string> GetPledgeTextAsync()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return settings.PledgeText ?? GetDefaultPledgeText();
    }

    /// <summary>
    /// Update pledge text
    /// </summary>
    public async Task UpdatePledgeTextAsync(string pledgeText, Guid modifiedByUserId)
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        settings.PledgeText = pledgeText;
        settings.ModifiedByUserId = modifiedByUserId;
        await _appSettingsRepository.UpdateSettingsAsync(settings);
    }

    /// <summary>
    /// Get default pledge text in Arabic
    /// </summary>
    private string GetDefaultPledgeText()
    {
        return @"بسم الله الرحمن الرحيم

الميثاق الطلابي

أتعهد بما يلي:
١. الالتزام بحفظ كتاب الله تعالى وتعلم تجويده
٢. احترام المعلمين والإدارة والزملاء
٣. المحافظة على أوقات الحضور والانصراف
٤. العناية بنظافة المصحف والمكان
٥. عدم الغياب إلا لعذر شرعي
٦. المحافظة على الهدوء والسكينة داخل الحلقات
٧. الالتزام بالآداب الإسلامية في التعامل

أسأل الله التوفيق والسداد
";
    }
}
