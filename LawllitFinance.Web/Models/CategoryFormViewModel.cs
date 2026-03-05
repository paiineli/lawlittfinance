using LawllitFinance.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace LawllitFinance.Web.Models;

public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Val_CatNameRequired")]
    [StringLength(100, ErrorMessage = "Val_CatNameMaxLength")]
    public string Name { get; set; } = string.Empty;

    public TransactionType Type { get; set; } = TransactionType.Expense;
}
