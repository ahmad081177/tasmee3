using QuranListeningApp.Domain.Entities;

namespace QuranListeningApp.Domain.Interfaces;

/// <summary>
/// Repository interface for SurahReference entity operations
/// </summary>
public interface ISurahReferenceRepository
{
    Task<SurahReference?> GetByNumberAsync(int surahNumber);
    Task<IEnumerable<SurahReference>> GetAllAsync();
    Task<IEnumerable<SurahReference>> GetMakkiSurahsAsync();
    Task<IEnumerable<SurahReference>> GetMadaniSurahsAsync();
    Task<bool> ExistsAsync(int surahNumber);
    Task<int> GetTotalCountAsync();
}
