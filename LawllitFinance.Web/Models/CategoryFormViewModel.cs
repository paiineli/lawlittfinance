using LawllitFinance.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace LawllitFinance.Web.Models;

public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    public TransactionType Type { get; set; } = TransactionType.Expense;
}
