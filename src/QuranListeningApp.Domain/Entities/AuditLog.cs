using System.ComponentModel.DataAnnotations;
using QuranListeningApp.Domain.Enums;

namespace QuranListeningApp.Domain.Entities;

/// <summary>
/// Tracks all Create, Update, Delete operations on critical entities
/// Used for security auditing and compliance
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who performed the action
    /// Foreign key to Users table
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user who performed the action
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Type of action performed (Created, Updated, Deleted, Viewed)
    /// </summary>
    [Required]
    public AuditAction Action { get; set; }

    /// <summary>
    /// Type of entity that was modified (User, ListeningSession, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was modified
    /// </summary>
    [Required]
    public Guid EntityId { get; set; }

    /// <summary>
    /// Old values before modification (JSON format)
    /// Null for Created actions
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// New values after modification (JSON format)
    /// Null for Deleted actions
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// When this action was performed
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the user who performed the action (optional)
    /// </summary>
    [MaxLength(45)] // IPv6 max length
    public string? IpAddress { get; set; }
}
