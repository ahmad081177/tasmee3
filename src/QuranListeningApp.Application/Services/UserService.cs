using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;

namespace QuranListeningApp.Application.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public UserService(IUserRepository userRepository, IAuditLogRepository auditLogRepository)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _userRepository.GetByRoleAsync(role);
    }

    public async Task<IEnumerable<User>> GetTeachersAsync()
    {
        return await _userRepository.GetTeachersAsync();
    }

    public async Task<IEnumerable<User>> GetStudentsAsync()
    {
        return await _userRepository.GetStudentsAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetActiveUsersAsync();
    }

    public async Task<User> CreateUserAsync(User user, Guid createdByUserId)
    {
        // Validate username doesn't exist
        if (await _userRepository.UsernameExistsAsync(user.Username))
        {
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");
        }

        // Validate ID number doesn't exist (if provided)
        if (!string.IsNullOrEmpty(user.IdNumber) && await _userRepository.IdNumberExistsAsync(user.IdNumber))
        {
            throw new InvalidOperationException($"ID number '{user.IdNumber}' already exists.");
        }

        // Hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedByUserId = createdByUserId;

        var createdUser = await _userRepository.CreateAsync(user);

        // Log audit
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = createdByUserId,
            Action = AuditAction.Created,
            EntityType = nameof(User),
            EntityId = createdUser.Id,
            NewValues = $"Username: {createdUser.Username}, Role: {createdUser.Role}"
        });

        return createdUser;
    }

    public async Task<User> UpdateUserAsync(User user, Guid modifiedByUserId)
    {
        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Check if username changed and validate
        if (existingUser.Username != user.Username && await _userRepository.UsernameExistsAsync(user.Username))
        {
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");
        }

        var updatedUser = await _userRepository.UpdateAsync(user);

        // Log audit
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = modifiedByUserId,
            Action = AuditAction.Updated,
            EntityType = nameof(User),
            EntityId = updatedUser.Id,
            OldValues = $"Username: {existingUser.Username}",
            NewValues = $"Username: {updatedUser.Username}"
        });

        return updatedUser;
    }

    public async Task DeleteUserAsync(Guid id, Guid deletedByUserId)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        await _userRepository.DeleteAsync(id);

        // Log audit
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = deletedByUserId,
            Action = AuditAction.Deleted,
            EntityType = nameof(User),
            EntityId = id,
            OldValues = $"Username: {user.Username}, Role: {user.Role}"
        });
    }

    public async Task<bool> ValidatePasswordAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }
    }

    public async Task AcceptPledgeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null && user.Role == UserRole.Student)
        {
            user.PledgeAcceptedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Log the pledge acceptance
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.Updated,
                EntityType = "User",
                EntityId = userId,
                NewValues = $"Pledge accepted at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public async Task<bool> HasAcceptedPledgeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.PledgeAcceptedDate != null;
    }

    public async Task<Dictionary<UserRole, int>> GetUserCountsByRoleAsync()
    {
        return new Dictionary<UserRole, int>
        {
            { UserRole.Admin, await _userRepository.GetCountByRoleAsync(UserRole.Admin) },
            { UserRole.Teacher, await _userRepository.GetCountByRoleAsync(UserRole.Teacher) },
            { UserRole.Student, await _userRepository.GetCountByRoleAsync(UserRole.Student) }
        };
    }
}
