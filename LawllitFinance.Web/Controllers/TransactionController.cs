using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;
using LawllitFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class TransactionController(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, int? month, int? year, string? search)
    {
        var userId = GetUserId();
        var now = DateTime.Now;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        var transactions = await transactionRepository.GetFilteredAsync(userId, type, selectedMonth, selectedYear);

        if (!string.IsNullOrWhiteSpace(search))
            transactions = transactions
                .Where(t => t.Description != null && t.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var categories = await categoryRepository.GetAllByUserAsync(userId);
        var pendingRecurringCount = await transactionRepository.GetPendingRecurringCountAsync(userId, selectedMonth, selectedYear);

        var viewModel = new TransactionListViewModel
        {
            Transactions = transactions,
            Categories = categories,
            FilterType = type,
            FilterSearch = search,
            FilterMonth = selectedMonth,
            FilterYear = selectedYear,
            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            PendingRecurringCount = pendingRecurringCount,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormViewModel transactionForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dados inválidos.";
            return RedirectToAction("Index");
        }

        var userId = GetUserId();
        var category = await categoryRepository.GetByIdAsync(userId, transactionForm.CategoryId);
        if (category is null)
        {
            TempData["Error"] = "Categoria não encontrada.";
            return RedirectToAction("Index");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Description = transactionForm.Description?.Trim() ?? "",
            Amount = transactionForm.Amount,
            Type = transactionForm.Type,
            Date = DateTime.SpecifyKind(transactionForm.Date.Date, DateTimeKind.Utc),
            UserId = userId,
            CategoryId = transactionForm.CategoryId,
            IsRecurring = transactionForm.IsRecurring,
            CreatedAt = DateTime.UtcNow,
        };

        await transactionRepository.AddAsync(transaction);
        await transactionRepository.SaveChangesAsync();

        TempData["Success"] = "Transação criada com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TransactionFormViewModel transactionForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dados inválidos.";
            return RedirectToAction("Index");
        }

        var userId = GetUserId();
        var transaction = await transactionRepository.GetByIdAsync(userId, id);
        if (transaction is null)
        {
            TempData["Error"] = "Transação não encontrada.";
            return RedirectToAction("Index");
        }

        var category = await categoryRepository.GetByIdAsync(userId, transactionForm.CategoryId);
        if (category is null)
        {
            TempData["Error"] = "Categoria não encontrada.";
            return RedirectToAction("Index");
        }

        transaction.Description = transactionForm.Description?.Trim() ?? "";
        transaction.Amount = transactionForm.Amount;
        transaction.Type = transactionForm.Type;
        transaction.Date = DateTime.SpecifyKind(transactionForm.Date.Date, DateTimeKind.Utc);
        transaction.CategoryId = transactionForm.CategoryId;
        transaction.IsRecurring = transactionForm.IsRecurring;

        await transactionRepository.SaveChangesAsync();

        TempData["Success"] = "Transação atualizada com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var transaction = await transactionRepository.GetByIdAsync(userId, id);
        if (transaction is null)
        {
            TempData["Error"] = "Transação não encontrada.";
            return RedirectToAction("Index");
        }

        await transactionRepository.DeleteAsync(transaction);
        await transactionRepository.SaveChangesAsync();

        TempData["Success"] = "Transação excluída com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportRecurring(int month, int year)
    {
        var userId = GetUserId();
        var previousTransactions = await transactionRepository.GetRecurringForImportAsync(userId, month, year);

        foreach (var previousTransaction in previousTransactions)
        {
            var newDate = new DateTime(year, month, Math.Min(previousTransaction.Date.Day, DateTime.DaysInMonth(year, month)));
            await transactionRepository.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                Description = previousTransaction.Description,
                Amount = previousTransaction.Amount,
                Type = previousTransaction.Type,
                Date = DateTime.SpecifyKind(newDate, DateTimeKind.Utc),
                UserId = userId,
                CategoryId = previousTransaction.CategoryId,
                IsRecurring = true,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await transactionRepository.SaveChangesAsync();

        var count = previousTransactions.Count;
        TempData["Success"] = count == 1
            ? "1 transação recorrente importada com sucesso."
            : $"{count} transações recorrentes importadas com sucesso.";

        return RedirectToAction("Index");
    }
}
