using QuranListeningApp.Domain.Entities;

namespace QuranListeningApp.Domain.Interfaces;

/// <summary>
/// Repository interface for ListeningSession entity operations
/// </summary>
public interface IListeningSessionRepository
{
    Task<ListeningSession?> GetByIdAsync(Guid id);
    Task<IEnumerable<ListeningSession>> GetAllAsync();
    Task<IEnumerable<ListeningSession>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<ListeningSession>> GetByTeacherIdAsync(Guid teacherId);
    Task<IEnumerable<ListeningSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ListeningSession>> GetSessionsByStudentIdAsync(Guid studentId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<ListeningSession>> GetSessionsByTeacherIdAsync(Guid teacherId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<ListeningSession>> GetSessionsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<ListeningSession>> GetCompletedSessionsAsync();
    Task<IEnumerable<ListeningSession>> GetPendingSessionsAsync();
    Task<IEnumerable<ListeningSession>> GetRecentSessionsAsync(int count);
    Task<ListeningSession> CreateAsync(ListeningSession session);
    Task<ListeningSession> UpdateAsync(ListeningSession session);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetTotalCountAsync();
    Task<int> GetCompletedCountAsync();
    Task<int> GetCountByStudentAsync(Guid studentId);
    Task<int> GetCountByTeacherAsync(Guid teacherId);
}
