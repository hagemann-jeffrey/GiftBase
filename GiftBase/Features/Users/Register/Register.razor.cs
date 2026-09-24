using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;

namespace GiftBase.Features.Users.Register;

public partial class Register(IAuthService authService)
{
    private RegisterInput RegisterInput { get; set; } = new RegisterInput();

    private bool Success { get; set; } = false;
    private bool IsLoading { get; set; } = false;
    private string? ErrorMessage { get; set; } = null;

    private async Task RegisterSubmit()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            await authService.RegisterUserAsync(RegisterInput.Email, RegisterInput.Password);

            Success = true;
        }
        catch (ConflictException)
        {
            ErrorMessage = "E-Mail ist bereits registriert oder es ist ein anderer Fehler aufgetreten.";
        }
        catch (Exception)
        {
            ErrorMessage = "Die Bestätigungs-E-Mail konnte nicht versendet werden. Bitte versuche es später erneut.";
        }

        IsLoading = false;
    }
}