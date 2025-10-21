namespace QuranListeningApp.Domain.Enums;

/// <summary>
/// Defines the roles available in the system for role-based access control
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator with full system access
    /// Can manage teachers, students, and view all records
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Teacher who records and manages listening sessions
    /// Can view all students and record sessions for any student
    /// Can only edit/delete their own sessions
    /// </summary>
    Teacher = 2,

    /// <summary>
    /// Student with read-only access to their own progress
    /// Can view their own listening session history
    /// </summary>
    Student = 3
}
