using LawllitFinance.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace LawllitFinance.Web.Models;

public class TransactionFormViewModel
{
    public Guid? Id { get; set; }

    [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Data é obrigatória")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Categoria é obrigatória")]
    public Guid CategoryId { get; set; }

    public bool IsRecurring { get; set; } = false;
}
