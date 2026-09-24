using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Users.ConfirmEmail;

public partial class ConfirmEmail(IAuthService authService)
{
    [SupplyParameterFromQuery]
    public required string Token { get; set; }

    private bool IsLoading = false;
    private bool IsSuccess = false;
    private string? ErrorMessage = null;

    private async Task ConfirmEmailClick()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Bestätigungstoken ist ungültig oder fehlt.";
            return;
        }

        ErrorMessage = null;
        IsLoading = true;

        try
        {
            await authService.ConfirmEmailAsync(Token);

            IsSuccess = true;
        }
        catch (NotFoundException)
        {
            ErrorMessage = "Die E-Mail-Bestätigung ist fehlgeschlagen. Bitte überprüfe den Bestätigungslink oder kontaktiere den Support.";
        }
        catch (ConflictException)
        {
            ErrorMessage = "Der Bestätigungslink ist abgelaufen. Bitte registriere dich erneut.";
        }

        IsLoading = false;
    }
}