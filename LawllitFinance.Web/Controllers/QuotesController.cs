using LawllitFinance.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class QuotesController(IQuotesService quotesService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var (quotes, errorTechDetails) = await quotesService.GetQuotesAsync();

        if (errorTechDetails is not null)
            TempData["Error"] = $"{localizer["Quote_LoadError"].Value} ({errorTechDetails})";

        return View(quotes);
    }
}
