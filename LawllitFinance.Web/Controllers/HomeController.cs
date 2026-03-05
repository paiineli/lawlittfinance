using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace LawllitFinance.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string language, string returnUrl)
    {
        if (!Constants.ValidLanguages.Contains(language))
            return BadRequest();

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture: "pt-BR", uiCulture: language)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
}
