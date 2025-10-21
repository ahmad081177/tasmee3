using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;

namespace QuranListeningApp.Domain.Interfaces;

/// <summary>
/// Repository interface for AuditLog entity operations
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count);
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<int> GetTotalCountAsync();
}
