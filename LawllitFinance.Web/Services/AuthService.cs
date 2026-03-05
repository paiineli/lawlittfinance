using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;

using LawllitFinance.Web.Services.Interfaces;

namespace LawllitFinance.Web.Services;

public class AuthService(IUserRepository userRepository) : IAuthService
{
    public async Task<(User? User, string? ErrorKey)> ValidateLoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (null, "Msg_WrongCredentials");
        if (!user.EmailConfirmed)
            return (null, "Msg_EmailNotConfirmed");
        return (user, null);
    }

    public async Task<(User? User, string? ErrorKey)> RegisterAsync(string name, string email, string password)
    {
        var existingUser = await userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            return (null, "Msg_EmailInUse");

        var confirmToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = StringHelpers.ToTitleCase(name),
            Email = email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            EmailConfirmed = false,
            EmailConfirmationToken = confirmToken,
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();
        return (user, null);
    }

    public async Task<User?> ConfirmEmailAsync(string token)
    {
        var user = await userRepository.GetByConfirmationTokenAsync(token);
        if (user is null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            return null;

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        await userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token);
        return user is not null && user.PasswordResetTokenExpiry >= DateTime.UtcNow;
    }

    public async Task<(User User, string Token)?> BeginPasswordResetAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null || !user.EmailConfirmed)
            return null;

        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await userRepository.SaveChangesAsync();
        return (user, resetToken);
    }

    public async Task<string?> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token);
        if (user is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return "Msg_InvalidLink";

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await userRepository.SaveChangesAsync();
        return null;
    }

    public async Task<User> GetOrCreateGoogleUserAsync(string googleId, string email, string name)
    {
        var user = await userRepository.GetByGoogleIdAsync(googleId)
                ?? await userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            user = new User { Id = Guid.NewGuid(), Name = name, Email = email, GoogleId = googleId, EmailConfirmed = true };
            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = googleId;
            user.EmailConfirmed = true;
            await userRepository.SaveChangesAsync();
        }

        return user;
    }
}
