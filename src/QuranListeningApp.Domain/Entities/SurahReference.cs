using System.ComponentModel.DataAnnotations;

namespace QuranListeningApp.Domain.Entities;

/// <summary>
/// Lookup table containing all 114 Surahs of the Quran
/// Used for validation and displaying Surah names
/// </summary>
public class SurahReference
{
    /// <summary>
    /// Surah number (1-114) - Primary Key
    /// </summary>
    [Key]
    [Range(1, 114)]
    public int SurahNumber { get; set; }

    /// <summary>
    /// Name of the Surah in Arabic
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SurahNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Name of the Surah in English (optional)
    /// </summary>
    [MaxLength(100)]
    public string? SurahNameEnglish { get; set; }

    /// <summary>
    /// Total number of Ayahs in this Surah
    /// Used for validation
    /// </summary>
    [Required]
    [Range(1, 286)] // Al-Baqarah has 286 ayahs (the longest)
    public int TotalAyahs { get; set; }

    /// <summary>
    /// Whether this Surah was revealed in Makkah (true) or Madinah (false)
    /// </summary>
    public bool IsMakki { get; set; }
}
