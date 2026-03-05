using LawllitFinance.Data.Entities;

namespace LawllitFinance.Web.Services.Interfaces;

public interface IAuthService
{
    Task<(User? User, string? ErrorKey)> ValidateLoginAsync(string email, string password);
    Task<(User? User, string? ErrorKey)> RegisterAsync(string name, string email, string password);
    Task<User?> ConfirmEmailAsync(string token);
    Task<bool> ValidatePasswordResetTokenAsync(string token);
    Task<(User User, string Token)?> BeginPasswordResetAsync(string email);
    Task<string?> ResetPasswordAsync(string token, string newPassword);
    Task<User> GetOrCreateGoogleUserAsync(string googleId, string email, string name);
}
