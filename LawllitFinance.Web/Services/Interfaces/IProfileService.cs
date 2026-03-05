using LawllitFinance.Data.Entities;
using LawllitFinance.Web.Models;

namespace LawllitFinance.Web.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileViewModel?> GetProfileAsync(Guid userId, string? tab);
    Task<(User? User, string? ErrorKey)> EditNameAsync(Guid userId, string name);
    Task<(User? User, string? ErrorKey)> EditEmailAsync(Guid userId, EditEmailViewModel form);
    Task<string?> ChangePasswordAsync(Guid userId, ChangePasswordViewModel form);
    Task<User?> SaveThemeAsync(Guid userId, string theme);
    Task<User?> SaveFontSizeAsync(Guid userId, string fontSize);
    Task<User?> SaveLanguageAsync(Guid userId, string language);
    Task<string?> DeleteAccountAsync(Guid userId, string? password);
}
