using LawllitFinance.Web.Models;

namespace LawllitFinance.Web.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionListViewModel> GetListViewModelAsync(Guid userId, string? type, int? month, int? year, string? search);
    Task<string?> CreateAsync(Guid userId, TransactionFormViewModel form);
    Task<string?> EditAsync(Guid userId, Guid id, TransactionFormViewModel form);
    Task<string?> DeleteAsync(Guid userId, Guid id);
    Task<int> ImportRecurringTransactionsAsync(Guid userId, int month, int year);
}
