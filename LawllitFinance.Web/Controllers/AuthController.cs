using LawllitFinance.Web.Models;
using LawllitFinance.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace LawllitFinance.Web.Controllers;

public class AuthController(IAuthService authService, IEmailService emailService, IStringLocalizer<SharedResource> localizer) : BaseController
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

        var (user, errorKey) = await authService.ValidateLoginAsync(loginForm.Email, loginForm.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, localizer[errorKey!]);
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

        var (user, errorKey) = await authService.RegisterAsync(registerForm.Name, registerForm.Email, registerForm.Password);
        if (user is null)
        {
            ModelState.AddModelError(nameof(registerForm.Email), localizer[errorKey!]);
            return View(registerForm);
        }

        var confirmUrl = Url.Action("ConfirmEmail", "Auth", new { token = user.EmailConfirmationToken }, Request.Scheme)!;
        await emailService.SendConfirmationEmailAsync(user.Email, user.Name, confirmUrl, user.Language);

        TempData["Success"] = localizer["Msg_EmailConfirmSent"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var user = await authService.ConfirmEmailAsync(token);
        if (user is null)
        {
            TempData["Error"] = localizer["Msg_InvalidLink"].Value;
            return RedirectToAction("Login");
        }

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

        var result = await authService.BeginPasswordResetAsync(forgotPasswordForm.Email.Trim().ToLower());
        if (result is not null)
        {
            var resetUrl = Url.Action("ResetPassword", "Auth", new { token = result.Value.Token }, Request.Scheme)!;
            await emailService.SendPasswordResetEmailAsync(result.Value.User.Email, result.Value.User.Name, resetUrl, result.Value.User.Language);
        }

        TempData["Success"] = localizer["Msg_ForgotEmailSent"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        if (!await authService.ValidatePasswordResetTokenAsync(token))
        {
            TempData["Error"] = localizer["Msg_InvalidLink"].Value;
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordForm)
    {
        if (!ModelState.IsValid) return View(resetPasswordForm);

        var errorKey = await authService.ResetPasswordAsync(resetPasswordForm.Token, resetPasswordForm.Password);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Login");
        }

        TempData["Success"] = localizer["Msg_PasswordReset"].Value;
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

        var name = StringHelpers.ToTitleCase(result.Principal!.FindFirstValue(ClaimTypes.Name) ?? email);
        var user = await authService.GetOrCreateGoogleUserAsync(googleId, email, name);

        await HttpContext.SignOutAsync("External");
        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }
}
