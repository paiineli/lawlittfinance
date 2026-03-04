using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;
using LawllitFinance.Web.Models;
using LawllitFinance.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LawllitFinance.Web.Controllers;

public class AuthController(IUserRepository userRepository, IEmailService emailService) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginForm)
    {
        if (!ModelState.IsValid) return View(loginForm);

        var user = await userRepository.GetByEmailAsync(loginForm.Email);
        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(loginForm.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return View(loginForm);
        }

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Confirme seu e-mail antes de entrar.");
            return View(loginForm);
        }

        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel registerForm)
    {
        if (!ModelState.IsValid) return View(registerForm);

        var existingUser = await userRepository.GetByEmailAsync(registerForm.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(registerForm.Email), "Este e-mail já está cadastrado.");
            return View(registerForm);
        }

        var confirmToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                       .ToTitleCase(registerForm.Name.Trim().ToLower()),
            Email = registerForm.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerForm.Password),
            EmailConfirmed = false,
            EmailConfirmationToken = confirmToken,
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        var confirmUrl = Url.Action("ConfirmEmail", "Auth", new { token = confirmToken }, Request.Scheme)!;
        await emailService.SendConfirmationEmailAsync(user.Email, user.Name, confirmUrl);

        TempData["Success"] = "Conta registrada! Verifique seu e-mail para ativar o acesso.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var user = await userRepository.GetByConfirmationTokenAsync(token);

        if (user is null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "Link inválido ou expirado.";
            return RedirectToAction("Login");
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        await userRepository.SaveChangesAsync();

        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordForm)
    {
        if (!ModelState.IsValid) return View(forgotPasswordForm);

        var user = await userRepository.GetByEmailAsync(forgotPasswordForm.Email.Trim().ToLower());
        if (user is not null && user.EmailConfirmed)
        {
            var resetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await userRepository.SaveChangesAsync();

            var resetUrl = Url.Action("ResetPassword", "Auth", new { token = resetToken }, Request.Scheme)!;
            await emailService.SendPasswordResetEmailAsync(user.Email, user.Name, resetUrl);
        }

        TempData["Success"] = "Se o e-mail estiver cadastrado, você receberá as instruções em breve.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token);
        if (user is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "Link inválido ou expirado.";
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordForm)
    {
        if (!ModelState.IsValid) return View(resetPasswordForm);

        var user = await userRepository.GetByPasswordResetTokenAsync(resetPasswordForm.Token);
        if (user is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "Link inválido ou expirado.";
            return RedirectToAction("Login");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordForm.Password);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await userRepository.SaveChangesAsync();

        TempData["Success"] = "Senha redefinida com sucesso! Faça login.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded) return RedirectToAction("Login");

        var googleId = result.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal!.FindFirstValue(ClaimTypes.Email);
        if (googleId is null || email is null) return RedirectToAction("Login");
        var rawNameFromOAuth = result.Principal!.FindFirstValue(ClaimTypes.Name) ?? email;
        var name = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                                   .ToTitleCase(rawNameFromOAuth.Trim().ToLower());

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

        await HttpContext.SignOutAsync("External");
        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
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
