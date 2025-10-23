using System.ComponentModel.DataAnnotations;

namespace QuranListeningApp.Domain.Entities;

/// <summary>
/// Stores application-wide settings and configuration
/// Implemented as a single-row table for school configuration
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Primary key (always 1 - single row table)
    /// </summary>
    [Key]
    public int Id { get; set; } = 1;

    /// <summary>
    /// School name in Arabic
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SchoolNameArabic { get; set; } = "مدرسة تحفيظ القرآن الكريم";

    /// <summary>
    /// Path to the school logo image (relative to wwwroot)
    /// e.g., "uploads/logo.png"
    /// </summary>
    [MaxLength(500)]
    public string? SchoolLogoPath { get; set; }

    /// <summary>
    /// Student pledge/covenant text (الميثاق) in Arabic
    /// Students must accept this on first login
    /// </summary>
    public string? PledgeText { get; set; }

    /// <summary>
    /// When settings were last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// User ID who last modified the settings
    /// </summary>
    public Guid? ModifiedByUserId { get; set; }
}
