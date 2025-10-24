using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbContextFactory<QuranAppDbContext> _contextFactory;

    public AuditLogRepository(IDbContextFactory<QuranAppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        auditLog.Timestamp = DateTime.UtcNow;
        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs.CountAsync();
    }
}
