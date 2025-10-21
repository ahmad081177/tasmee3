using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;

namespace QuranListeningApp.Application.Services;

/// <summary>
/// Service for listening session management
/// </summary>
public class ListeningSessionService
{
    private readonly IListeningSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISurahReferenceRepository _surahRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ListeningSessionService(
        IListeningSessionRepository sessionRepository,
        IUserRepository userRepository,
        ISurahReferenceRepository surahRepository,
        IAuditLogRepository auditLogRepository)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _surahRepository = surahRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ListeningSession?> GetSessionByIdAsync(Guid id)
    {
        return await _sessionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ListeningSession>> GetAllSessionsAsync()
    {
        return await _sessionRepository.GetAllAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetSessionsByStudentAsync(Guid studentId)
    {
        return await _sessionRepository.GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<ListeningSession>> GetSessionsByTeacherAsync(Guid teacherId)
    {
        return await _sessionRepository.GetByTeacherIdAsync(teacherId);
    }

    public async Task<IEnumerable<ListeningSession>> GetRecentSessionsAsync(int count = 10)
    {
        return await _sessionRepository.GetRecentSessionsAsync(count);
    }

    public async Task<ListeningSession> CreateSessionAsync(ListeningSession session, Guid createdByUserId)
    {
        // Validate student exists
        var student = await _userRepository.GetByIdAsync(session.StudentUserId);
        if (student == null || student.Role != UserRole.Student)
        {
            throw new InvalidOperationException("Invalid student ID.");
        }

        // Validate teacher exists
        var teacher = await _userRepository.GetByIdAsync(session.TeacherUserId);
        if (teacher == null || teacher.Role != UserRole.Teacher)
        {
            throw new InvalidOperationException("Invalid teacher ID.");
        }

        // Validate Surah number
        if (!await _surahRepository.ExistsAsync(session.SurahNumber))
        {
            throw new InvalidOperationException($"Invalid Surah number: {session.SurahNumber}");
        }

        var createdSession = await _sessionRepository.CreateAsync(session);

        // Log audit
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = createdByUserId,
            Action = AuditAction.Created,
            EntityType = nameof(ListeningSession),
            EntityId = createdSession.Id,
            NewValues = $"Student: {student.FullNameArabic}, Teacher: {teacher.FullNameArabic}, Date: {session.SessionDate:yyyy-MM-dd}"
        });

        return createdSession;
    }

    public async Task<ListeningSession> UpdateSessionAsync(ListeningSession session, Guid modifiedByUserId)
    {
        var existingSession = await _sessionRepository.GetByIdAsync(session.Id);
        if (existingSession == null)
        {
            throw new InvalidOperationException("Session not found.");
        }

        var updatedSession = await _sessionRepository.UpdateAsync(session);

        // Log audit
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = modifiedByUserId,
            Action = AuditAction.Updated,
            EntityType = nameof(ListeningSession),
            EntityId = updatedSession.Id,
            NewValues = $"Session updated - IsCompleted: {updatedSession.IsCompleted}"
        });

        return updatedSession;
    }

    public async Task DeleteSessionAsync(Guid id, Guid deletedByUserId)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found.");
        }

        await _sessionRepository.DeleteAsync(id);

        // Log audit
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = deletedByUserId,
            Action = AuditAction.Deleted,
            EntityType = nameof(ListeningSession),
            EntityId = id,
            OldValues = $"Session for student ID: {session.StudentUserId}"
        });
    }

    public async Task<int> GetSessionCountAsync()
    {
        return await _sessionRepository.GetTotalCountAsync();
    }

    public async Task<int> GetCompletedSessionCountAsync()
    {
        return await _sessionRepository.GetCompletedCountAsync();
    }

    public async Task<Dictionary<string, int>> GetSessionStatisticsAsync()
    {
        return new Dictionary<string, int>
        {
            { "Total", await _sessionRepository.GetTotalCountAsync() },
            { "Completed", await _sessionRepository.GetCompletedCountAsync() },
            { "Pending", await _sessionRepository.GetTotalCountAsync() - await _sessionRepository.GetCompletedCountAsync() }
        };
    }
}
