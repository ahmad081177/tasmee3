using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly QuranAppDbContext _context;

    public AuditLogRepository(QuranAppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        auditLog.Timestamp = DateTime.UtcNow;
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.AuditLogs.CountAsync();
    }
}
