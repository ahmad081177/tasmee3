using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;

namespace QuranListeningApp.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdNumberAsync(string idNumber);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetTeachersAsync();
    Task<IEnumerable<User>> GetStudentsAsync();
    Task<IEnumerable<User>> GetStudentsByTeacherAsync(Guid teacherId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> IdNumberExistsAsync(string idNumber);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByRoleAsync(UserRole role);
}
