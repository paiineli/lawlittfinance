using System.ComponentModel.DataAnnotations;

namespace LawllitFinance.Web.Models;

public class ProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public DateTime MemberSince { get; set; }
    public string ActiveTab { get; set; } = "info";
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
}

public class SaveThemeViewModel
{
    [Required]
    public string Theme { get; set; } = string.Empty;
}

public class SaveFontSizeViewModel
{
    [Required]
    public string FontSize { get; set; } = string.Empty;
}

public class EditNameViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class EditEmailViewModel
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "Mínimo de 6 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [Compare(nameof(NewPassword), ErrorMessage = "As senhas não conferem.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
