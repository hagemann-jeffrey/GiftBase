using System.ComponentModel.DataAnnotations;
using GiftBase.Shared.Validation;

namespace GiftBase.Features.Users.Register;

public class RegisterInput
{
    [Required(ErrorMessage = "Bitte gib deine E-Mail-Adresse ein.")]
    [EmailAddress(ErrorMessage = "Das ist keine gültige E-Mail.")]
    [MaxLength(100, ErrorMessage = "Die E-Mail darf maximal 100 Zeichen lang sein.")]
    [IuEmail]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Bitte gib dein Passwort ein.")]
    [MinLength(8, ErrorMessage = "Das Passwort muss mindestens 8 Zeichen lang sein.")]
    public string Password { get; set; } = null!;
    [Required(ErrorMessage = "Bitte bestätige dein Passwort.")]
    [Compare(nameof(Password), ErrorMessage = "Die Passwörter stimmen nicht überein.")]
    public string ConfirmPassword { get; set; } = null!;
}
