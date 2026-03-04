using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;
using LawllitFinance.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class ProfileController(IUserRepository userRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tab)
    {
        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return RedirectToAction("Logout", "Auth");

        var viewModel = new ProfileViewModel
        {
            Name        = user.Name,
            Email       = user.Email,
            HasPassword = user.PasswordHash is not null,
            MemberSince = user.CreatedAt,
            ActiveTab   = tab ?? "info",
            Theme       = user.Theme,
            FontSize    = user.FontSize,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditName(EditNameViewModel editNameForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values
                .SelectMany(value => value.Errors)
                .FirstOrDefault()?.ErrorMessage ?? "Nome inválido.";
            return RedirectToAction("Index");
        }

        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return RedirectToAction("Logout", "Auth");

        user.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(editNameForm.Name.Trim().ToLower());
        await userRepository.SaveChangesAsync();
        await SignInAsync(user);

        TempData["Success"] = "Nome atualizado com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmail(EditEmailViewModel editEmailForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values
                .SelectMany(value => value.Errors)
                .FirstOrDefault()?.ErrorMessage ?? "E-mail inválido.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return RedirectToAction("Logout", "Auth");

        if (user.PasswordHash is not null &&
            (string.IsNullOrEmpty(editEmailForm.Password) || !BCrypt.Net.BCrypt.Verify(editEmailForm.Password, user.PasswordHash)))
        {
            TempData["Error"] = "Senha incorreta.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        var normalizedEmail = editEmailForm.Email.Trim().ToLower();

        if (user.Email == normalizedEmail)
        {
            TempData["Error"] = "O novo e-mail é igual ao atual.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            TempData["Error"] = "Este e-mail já está em uso.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        user.Email = normalizedEmail;
        await userRepository.SaveChangesAsync();
        await SignInAsync(user);

        TempData["Success"] = "E-mail atualizado com sucesso.";
        return RedirectToAction("Index", new { tab = "security" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values
                .SelectMany(value => value.Errors)
                .FirstOrDefault()?.ErrorMessage ?? "Dados inválidos.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null || user.PasswordHash is null)
        {
            TempData["Error"] = "Operação não permitida.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        if (!BCrypt.Net.BCrypt.Verify(changePasswordForm.CurrentPassword, user.PasswordHash))
        {
            TempData["Error"] = "Senha atual incorreta.";
            return RedirectToAction("Index", new { tab = "security" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordForm.NewPassword);
        await userRepository.SaveChangesAsync();

        TempData["Success"] = "Senha alterada com sucesso.";
        return RedirectToAction("Index", new { tab = "security" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTheme(SaveThemeViewModel form)
    {
        string[] allowed = ["dark", "light", "high-contrast"];
        if (!ModelState.IsValid || !allowed.Contains(form.Theme))
            return BadRequest();

        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        user.Theme = form.Theme;
        await userRepository.SaveChangesAsync();
        await SignInAsync(user);

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFontSize(SaveFontSizeViewModel form)
    {
        string[] allowed = ["normal", "large", "xlarge"];
        if (!ModelState.IsValid || !allowed.Contains(form.FontSize))
            return BadRequest();

        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        user.FontSize = form.FontSize;
        await userRepository.SaveChangesAsync();
        await SignInAsync(user);

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string? password)
    {
        var userId = GetUserId();
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return RedirectToAction("Logout", "Auth");

        if (user.PasswordHash is not null && !BCrypt.Net.BCrypt.Verify(password ?? "", user.PasswordHash))
        {
            TempData["Error"] = "Senha incorreta.";
            return RedirectToAction("Index", new { tab = "account" });
        }

        await userRepository.DeleteAsync(user);
        await userRepository.SaveChangesAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
            new("theme",                   user.Theme),
            new("font_size",               user.FontSize),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }
}
