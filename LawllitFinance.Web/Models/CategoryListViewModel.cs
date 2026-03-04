using LawllitFinance.Data.Entities;

namespace LawllitFinance.Web.Models;

public class CategoryListViewModel
{
    public List<Category> Categories { get; set; } = [];
    public string? FilterType { get; set; }
    public string? FilterSearch { get; set; }
}
