using LawllitFinance.Web.Models;
using LawllitFinance.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class CategoryController(ICategoryService categoryService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, string? search)
    {
        var viewModel = await categoryService.GetListViewModelAsync(GetUserId(), type, search);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel categoryForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = localizer["Msg_CatNameRequired"].Value;
            return RedirectToAction("Index");
        }

        var errorKey = await categoryService.CreateAsync(GetUserId(), categoryForm);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_CatCreated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormViewModel categoryForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = localizer["Msg_CatNameRequired"].Value;
            return RedirectToAction("Index");
        }

        var errorKey = await categoryService.EditAsync(GetUserId(), id, categoryForm);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_CatUpdated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var errorKey = await categoryService.DeleteAsync(GetUserId(), id);
        if (errorKey is not null)
        {
            TempData["Error"] = localizer[errorKey].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_CatDeleted"].Value;
        return RedirectToAction("Index");
    }
}
