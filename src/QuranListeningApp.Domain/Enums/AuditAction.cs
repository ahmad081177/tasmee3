namespace QuranListeningApp.Domain.Enums;

/// <summary>
/// Defines the types of audit actions that can be logged
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Entity was created
    /// </summary>
    Created = 1,

    /// <summary>
    /// Entity was updated/modified
    /// </summary>
    Updated = 2,

    /// <summary>
    /// Entity was deleted
    /// </summary>
    Deleted = 3,

    /// <summary>
    /// Entity was viewed (optional, for high-security scenarios)
    /// </summary>
    Viewed = 4
}
