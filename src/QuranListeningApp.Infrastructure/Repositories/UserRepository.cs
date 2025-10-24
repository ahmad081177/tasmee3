using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<QuranAppDbContext> _contextFactory;

    public UserRepository(IDbContextFactory<QuranAppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .Include(u => u.CreatedByUser)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdNumberAsync(string idNumber)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .FirstOrDefaultAsync(u => u.IdNumber == idNumber);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .Include(u => u.CreatedByUser)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .Where(u => u.Role == role)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetTeachersAsync()
    {
        return await GetByRoleAsync(UserRole.Teacher);
    }

    public async Task<IEnumerable<User>> GetStudentsAsync()
    {
        return await GetByRoleAsync(UserRole.Student);
    }

    public async Task<IEnumerable<User>> GetStudentsByTeacherAsync(Guid teacherId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        // Get students who have sessions with this teacher
        var studentIds = await context.ListeningSessions
            .Where(s => s.TeacherUserId == teacherId)
            .Select(s => s.StudentUserId)
            .Distinct()
            .ToListAsync();

        return await context.Users
            .Where(u => studentIds.Contains(u.Id))
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        user.CreatedDate = DateTime.UtcNow;
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        user.ModifiedDate = DateTime.UtcNow;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> IdNumberExistsAsync(string idNumber)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.AnyAsync(u => u.IdNumber == idNumber);
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.CountAsync();
    }

    public async Task<int> GetCountByRoleAsync(UserRole role)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.CountAsync(u => u.Role == role);
    }
}
