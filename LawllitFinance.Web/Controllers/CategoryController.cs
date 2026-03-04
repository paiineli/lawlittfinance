using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;
using LawllitFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class CategoryController(ICategoryRepository categoryRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, string? search)
    {
        var userId = GetUserId();
        var categories = await categoryRepository.GetAllByUserAsync(userId);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, out var parsedType))
            categories = categories.Where(c => c.Type == parsedType).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            categories = categories
                .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var viewModel = new CategoryListViewModel
        {
            Categories = categories,
            FilterType = type,
            FilterSearch = search,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel categoryForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Nome da categoria é obrigatório.";
            return RedirectToAction("Index");
        }

        var userId = GetUserId();

        if (await categoryRepository.ExistsAsync(userId, categoryForm.Name, categoryForm.Type))
        {
            TempData["Error"] = "Já existe uma categoria com este nome e tipo.";
            return RedirectToAction("Index");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = categoryForm.Name.Trim(),
            Type = categoryForm.Type,
            UserId = userId,
        };

        await categoryRepository.AddAsync(category);
        await categoryRepository.SaveChangesAsync();

        TempData["Success"] = "Categoria criada com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormViewModel categoryForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Nome da categoria é obrigatório.";
            return RedirectToAction("Index");
        }

        var userId = GetUserId();
        var category = await categoryRepository.GetByIdAsync(userId, id);
        if (category is null)
        {
            TempData["Error"] = "Categoria não encontrada.";
            return RedirectToAction("Index");
        }

        if (await categoryRepository.ExistsAsync(userId, categoryForm.Name, categoryForm.Type, excludeId: id))
        {
            TempData["Error"] = "Já existe uma categoria com este nome e tipo.";
            return RedirectToAction("Index");
        }

        category.Name = categoryForm.Name.Trim();
        category.Type = categoryForm.Type;
        await categoryRepository.SaveChangesAsync();

        TempData["Success"] = "Categoria atualizada com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var category = await categoryRepository.GetByIdAsync(userId, id);
        if (category is null)
        {
            TempData["Error"] = "Categoria não encontrada.";
            return RedirectToAction("Index");
        }

        if (await categoryRepository.HasTransactionsAsync(userId, id))
        {
            TempData["Error"] = "Não é possível excluir uma categoria com transações vinculadas.";
            return RedirectToAction("Index");
        }

        await categoryRepository.DeleteAsync(category);
        await categoryRepository.SaveChangesAsync();

        TempData["Success"] = "Categoria excluída com sucesso.";
        return RedirectToAction("Index");
    }
}
