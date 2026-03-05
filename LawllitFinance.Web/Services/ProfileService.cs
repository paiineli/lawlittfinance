using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;
using LawllitFinance.Web.Models;

using LawllitFinance.Web.Services.Interfaces;

namespace LawllitFinance.Web.Services;

public class ProfileService(IUserRepository userRepository) : IProfileService
{
    public async Task<ProfileViewModel?> GetProfileAsync(Guid userId, string? tab)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        return new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email,
            HasPassword = user.PasswordHash is not null,
            MemberSince = user.CreatedAt,
            ActiveTab = tab ?? "info",
            Theme = user.Theme,
            FontSize = user.FontSize,
            Language = user.Language,
        };
    }

    public async Task<(User? User, string? ErrorKey)> EditNameAsync(Guid userId, string name)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return (null, null);

        user.Name = StringHelpers.ToTitleCase(name);
        await userRepository.SaveChangesAsync();
        return (user, null);
    }

    public async Task<(User? User, string? ErrorKey)> EditEmailAsync(Guid userId, EditEmailViewModel form)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return (null, null);

        if (user.PasswordHash is not null &&
            (string.IsNullOrEmpty(form.Password) || !BCrypt.Net.BCrypt.Verify(form.Password, user.PasswordHash)))
            return (null, "Msg_WrongPassword");

        var normalizedEmail = form.Email.Trim().ToLower();

        if (user.Email == normalizedEmail)
            return (null, "Msg_EmailSameAsCurrent");

        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            return (null, "Msg_EmailInUse");

        user.Email = normalizedEmail;
        await userRepository.SaveChangesAsync();
        return (user, null);
    }

    public async Task<string?> ChangePasswordAsync(Guid userId, ChangePasswordViewModel form)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null || user.PasswordHash is null)
            return "Msg_OperationNotAllowed";

        if (!BCrypt.Net.BCrypt.Verify(form.CurrentPassword, user.PasswordHash))
            return "Msg_CurrentPasswordWrong";

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(form.NewPassword);
        await userRepository.SaveChangesAsync();
        return null;
    }

    public async Task<User?> SaveThemeAsync(Guid userId, string theme)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        user.Theme = theme;
        await userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<User?> SaveFontSizeAsync(Guid userId, string fontSize)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        user.FontSize = fontSize;
        await userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<User?> SaveLanguageAsync(Guid userId, string language)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        user.Language = language;
        await userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<string?> DeleteAccountAsync(Guid userId, string? password)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        if (user.PasswordHash is not null && !BCrypt.Net.BCrypt.Verify(password ?? "", user.PasswordHash))
            return "Msg_WrongPassword";

        await userRepository.DeleteAsync(user);
        await userRepository.SaveChangesAsync();
        return null;
    }
}
