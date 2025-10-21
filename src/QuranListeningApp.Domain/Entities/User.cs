using System.ComponentModel.DataAnnotations;
using QuranListeningApp.Domain.Enums;

namespace QuranListeningApp.Domain.Entities;

/// <summary>
/// Represents a user in the system (Admin, Teacher, or Student)
/// Single table for all user types - optimized design
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username for login (unique)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Full name in Arabic
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FullNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// National ID number (required for Teachers and Students, nullable for Admin)
    /// </summary>
    [MaxLength(20)]
    public string? IdNumber { get; set; }

    /// <summary>
    /// Phone number (required for Teachers and Students, nullable for Admin)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email address (optional)
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// User role (Admin, Teacher, Student)
    /// </summary>
    [Required]
    public UserRole Role { get; set; }

    /// <summary>
    /// Grade level (only for Students, null for Admin/Teacher)
    /// </summary>
    [MaxLength(50)]
    public string? GradeLevel { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user account was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// ID of the admin user who created this account
    /// Self-referential foreign key
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Navigation property to the admin who created this user
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Last successful login timestamp
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    // Navigation properties

    /// <summary>
    /// Listening sessions where this user is the student
    /// </summary>
    public ICollection<ListeningSession> SessionsAsStudent { get; set; } = new List<ListeningSession>();

    /// <summary>
    /// Listening sessions where this user is the teacher
    /// </summary>
    public ICollection<ListeningSession> SessionsAsTeacher { get; set; } = new List<ListeningSession>();

    /// <summary>
    /// Users created by this user (if this user is an admin)
    /// </summary>
    public ICollection<User> CreatedUsers { get; set; } = new List<User>();
}
