using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;

namespace QuranListeningApp.Application.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AuthService(IUserRepository userRepository, IAuditLogRepository auditLogRepository)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<User?> AuthenticateAsync(string username, string password, string? ipAddress = null)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null || !user.IsActive)
        {
            // Log failed login attempt
            if (user != null)
            {
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = AuditAction.Viewed,
                    EntityType = "Authentication",
                    EntityId = user.Id,
                    NewValues = "Failed login attempt - Inactive account",
                    IpAddress = ipAddress
                });
            }
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            // Log failed login attempt
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = user.Id,
                Action = AuditAction.Viewed,
                EntityType = "Authentication",
                EntityId = user.Id,
                NewValues = "Failed login attempt - Invalid password",
                IpAddress = ipAddress
            });
            return null;
        }

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Log successful login
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = user.Id,
            Action = AuditAction.Viewed,
            EntityType = "Authentication",
            EntityId = user.Id,
            NewValues = "Successful login",
            IpAddress = ipAddress
        });

        return user;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        // Log password change
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = AuditAction.Updated,
            EntityType = "Password",
            EntityId = userId,
            NewValues = "Password changed"
        });

        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword, Guid resetByUserId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        // Log password reset
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserId = resetByUserId,
            Action = AuditAction.Updated,
            EntityType = "Password",
            EntityId = userId,
            NewValues = $"Password reset for user {user.Username}"
        });

        return true;
    }
}
