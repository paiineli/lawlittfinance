using System.ComponentModel.DataAnnotations;

namespace LawllitFinance.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_PasswordRequired")]
    public string Password { get; set; } = string.Empty;
}
