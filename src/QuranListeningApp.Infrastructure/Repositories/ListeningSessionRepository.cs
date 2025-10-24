using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class ListeningSessionRepository : IListeningSessionRepository
{
    private readonly IDbContextFactory<QuranAppDbContext> _contextFactory;

    public ListeningSessionRepository(IDbContextFactory<QuranAppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ListeningSession?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<ListeningSession>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetByStudentIdAsync(Guid studentId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.StudentUserId == studentId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetByTeacherIdAsync(Guid teacherId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.TeacherUserId == teacherId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.SessionDate >= startDate && s.SessionDate <= endDate)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetSessionsByStudentIdAsync(Guid studentId, DateTime fromDate, DateTime toDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.StudentUserId == studentId && s.SessionDate >= fromDate && s.SessionDate <= toDate)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetSessionsByTeacherIdAsync(Guid teacherId, DateTime fromDate, DateTime toDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.TeacherUserId == teacherId && s.SessionDate >= fromDate && s.SessionDate <= toDate)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetSessionsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.SessionDate >= fromDate && s.SessionDate <= toDate)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetCompletedSessionsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.IsCompleted)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetPendingSessionsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => !s.IsCompleted)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetRecentSessionsAsync(int count)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .OrderByDescending(s => s.SessionDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ListeningSession> CreateAsync(ListeningSession session)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        session.CreatedDate = DateTime.UtcNow;
        context.ListeningSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<ListeningSession> UpdateAsync(ListeningSession session)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        session.ModifiedDate = DateTime.UtcNow;
        context.ListeningSessions.Update(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var session = await context.ListeningSessions.FindAsync(id);
        if (session != null)
        {
            context.ListeningSessions.Remove(session);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions.AnyAsync(s => s.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions.CountAsync();
    }

    public async Task<int> GetCompletedCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions.CountAsync(s => s.IsCompleted);
    }

    public async Task<int> GetCountByStudentAsync(Guid studentId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions.CountAsync(s => s.StudentUserId == studentId);
    }

    public async Task<int> GetCountByTeacherAsync(Guid teacherId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ListeningSessions.CountAsync(s => s.TeacherUserId == teacherId);
    }
}
