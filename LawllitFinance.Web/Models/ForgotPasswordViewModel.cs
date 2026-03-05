using System.ComponentModel.DataAnnotations;

namespace LawllitFinance.Web.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;
}
