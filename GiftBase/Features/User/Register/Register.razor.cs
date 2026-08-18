using GiftBase.Core.Interfaces;

namespace GiftBase.Features.User.Register;

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

        var success = await authService.RegisterUserAsync(RegisterInput.Email, RegisterInput.Password);

        if (success)
        {
            Success = true;
        }
        else
        {
            ErrorMessage = "E-Mail ist bereits registriert oder es ist ein anderer Fehler aufgetreten.";
        }
        IsLoading = false;
    }
}