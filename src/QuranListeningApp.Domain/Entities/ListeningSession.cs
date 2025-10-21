using System.ComponentModel.DataAnnotations;

namespace QuranListeningApp.Domain.Entities;

/// <summary>
/// Represents a Quran recitation listening session
/// Records when a teacher examines a student's memorization
/// </summary>
public class ListeningSession
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the student being examined
    /// Foreign key to Users table (must have Role=Student)
    /// </summary>
    [Required]
    public Guid StudentUserId { get; set; }

    /// <summary>
    /// Navigation property to the student user
    /// </summary>
    public User Student { get; set; } = null!;

    /// <summary>
    /// ID of the teacher conducting the session
    /// Foreign key to Users table (must have Role=Teacher)
    /// </summary>
    [Required]
    public Guid TeacherUserId { get; set; }

    /// <summary>
    /// Navigation property to the teacher user
    /// </summary>
    public User Teacher { get; set; } = null!;

    /// <summary>
    /// Date and time of the session
    /// Defaults to current date/time but can be modified by teacher
    /// </summary>
    [Required]
    public DateTime SessionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Surah number (1-114)
    /// Each session covers one Surah only
    /// </summary>
    [Required]
    [Range(1, 114)]
    public int SurahNumber { get; set; }

    /// <summary>
    /// Starting Ayah number within the Surah
    /// Must be positive and valid for the Surah
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int FromAyahNumber { get; set; }

    /// <summary>
    /// Ending Ayah number within the Surah
    /// Must be >= FromAyahNumber and valid for the Surah
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int ToAyahNumber { get; set; }

    /// <summary>
    /// Count of major errors (خطأ جلي - Clear/Obvious Error)
    /// Must be non-negative
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MajorErrorsCount { get; set; } = 0;

    /// <summary>
    /// Count of minor errors (خطأ خفي - Hidden/Subtle Error)
    /// Must be non-negative
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MinorErrorsCount { get; set; } = 0;

    /// <summary>
    /// Whether the student passed/completed this session successfully
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Teacher's observations and comments in Arabic
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this session record was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this session record was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}
