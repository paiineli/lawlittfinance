using LawllitFinance.Web.Models;

namespace LawllitFinance.Web.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryListViewModel> GetListViewModelAsync(Guid userId, string? type, string? search);
    Task<string?> CreateAsync(Guid userId, CategoryFormViewModel form);
    Task<string?> EditAsync(Guid userId, Guid id, CategoryFormViewModel form);
    Task<string?> DeleteAsync(Guid userId, Guid id);
}
