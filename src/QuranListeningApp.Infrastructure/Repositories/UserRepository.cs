using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly QuranAppDbContext _context;

    public UserRepository(QuranAppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.CreatedByUser)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdNumberAsync(string idNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.IdNumber == idNumber);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.CreatedByUser)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
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
        // Get students who have sessions with this teacher
        var studentIds = await _context.ListeningSessions
            .Where(s => s.TeacherUserId == teacherId)
            .Select(s => s.StudentUserId)
            .Distinct()
            .ToListAsync();

        return await _context.Users
            .Where(u => studentIds.Contains(u.Id))
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.ModifiedDate = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> IdNumberExistsAsync(string idNumber)
    {
        return await _context.Users.AnyAsync(u => u.IdNumber == idNumber);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetCountByRoleAsync(UserRole role)
    {
        return await _context.Users.CountAsync(u => u.Role == role);
    }
}
