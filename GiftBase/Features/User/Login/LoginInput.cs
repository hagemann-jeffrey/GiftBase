using System.ComponentModel.DataAnnotations;

namespace GiftBase.Features.User.Login;

public class LoginInput
{
    [Required(ErrorMessage = "Bitte gib deine E-Mail-Adresse ein.")]
    [EmailAddress(ErrorMessage = "Das ist keine gültige E-Mail.")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Bitte gib dein Passwort ein.")]
    public string Password { get; set; } = null!;
}
