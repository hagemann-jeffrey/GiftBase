using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Users.Login;

public partial class Login
{
    private LoginInput LoginInput { get; set; } = new();

    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    private string? ErrorMessage => Error == "invalid"
        ? "E-Mail oder Passwort ist falsch, oder der Account ist noch nicht bestätigt."
        : null;
}