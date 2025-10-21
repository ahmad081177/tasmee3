using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class ListeningSessionRepository : IListeningSessionRepository
{
    private readonly QuranAppDbContext _context;

    public ListeningSessionRepository(QuranAppDbContext context)
    {
        _context = context;
    }

    public async Task<ListeningSession?> GetByIdAsync(Guid id)
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<ListeningSession>> GetAllAsync()
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.StudentUserId == studentId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetByTeacherIdAsync(Guid teacherId)
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.TeacherUserId == teacherId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.SessionDate >= startDate && s.SessionDate <= endDate)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetCompletedSessionsAsync()
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => s.IsCompleted)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetPendingSessionsAsync()
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Where(s => !s.IsCompleted)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListeningSession>> GetRecentSessionsAsync(int count)
    {
        return await _context.ListeningSessions
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .OrderByDescending(s => s.SessionDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ListeningSession> CreateAsync(ListeningSession session)
    {
        session.CreatedDate = DateTime.UtcNow;
        _context.ListeningSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<ListeningSession> UpdateAsync(ListeningSession session)
    {
        session.ModifiedDate = DateTime.UtcNow;
        _context.ListeningSessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task DeleteAsync(Guid id)
    {
        var session = await _context.ListeningSessions.FindAsync(id);
        if (session != null)
        {
            _context.ListeningSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ListeningSessions.AnyAsync(s => s.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.ListeningSessions.CountAsync();
    }

    public async Task<int> GetCompletedCountAsync()
    {
        return await _context.ListeningSessions.CountAsync(s => s.IsCompleted);
    }

    public async Task<int> GetCountByStudentAsync(Guid studentId)
    {
        return await _context.ListeningSessions.CountAsync(s => s.StudentUserId == studentId);
    }

    public async Task<int> GetCountByTeacherAsync(Guid teacherId)
    {
        return await _context.ListeningSessions.CountAsync(s => s.TeacherUserId == teacherId);
    }
}
