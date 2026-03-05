using LawllitFinance.Web.Models;
using LawllitFinance.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class ProfileController(IProfileService profileService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tab)
    {
        var viewModel = await profileService.GetProfileAsync(GetUserId(), tab);
        if (viewModel is null)
            return RedirectToAction("Logout", "Auth");

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
                .FirstOrDefault()?.ErrorMessage ?? localizer["Msg_NameInvalid"].Value;
            return RedirectToAction("Index");
        }

        var (user, errorKey) = await profileService.EditNameAsync(GetUserId(), editNameForm.Name);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index");
        }

        if (user is not null)
            await SignInAsync(user);

        TempData["Success"] = localizer["Msg_NameUpdated"].Value;
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
                .FirstOrDefault()?.ErrorMessage ?? localizer["Msg_EmailInvalid"].Value;
            return RedirectToAction("Index", new { tab = "security" });
        }

        var (user, errorKey) = await profileService.EditEmailAsync(GetUserId(), editEmailForm);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index", new { tab = "security" });
        }

        if (user is not null)
            await SignInAsync(user);

        TempData["Success"] = localizer["Msg_EmailUpdated"].Value;
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
                .FirstOrDefault()?.ErrorMessage ?? localizer["Msg_DataInvalid"].Value;
            return RedirectToAction("Index", new { tab = "security" });
        }

        var errorKey = await profileService.ChangePasswordAsync(GetUserId(), changePasswordForm);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index", new { tab = "security" });
        }

        TempData["Success"] = localizer["Msg_PasswordChanged"].Value;
        return RedirectToAction("Index", new { tab = "security" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTheme(SaveThemeViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidThemes.Contains(form.Theme))
            return BadRequest();

        var user = await profileService.SaveThemeAsync(GetUserId(), form.Theme);
        if (user is null)
            return Unauthorized();

        await SignInAsync(user);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFontSize(SaveFontSizeViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidFontSizes.Contains(form.FontSize))
            return BadRequest();

        var user = await profileService.SaveFontSizeAsync(GetUserId(), form.FontSize);
        if (user is null)
            return Unauthorized();

        await SignInAsync(user);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLanguage(SaveLanguageViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidLanguages.Contains(form.Language))
            return BadRequest();

        var user = await profileService.SaveLanguageAsync(GetUserId(), form.Language);
        if (user is null)
            return Unauthorized();

        await SignInAsync(user);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string? password)
    {
        var errorKey = await profileService.DeleteAccountAsync(GetUserId(), password);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index", new { tab = "account" });
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
