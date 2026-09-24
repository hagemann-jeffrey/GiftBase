using System.Security.Claims;
using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace GiftBase.Features.Users.Login;

public static class LoginEndpoints
{
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/login", HandleLoginAsync);
        endpoints.MapPost("/logout", (Delegate)HandleLogoutAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleLoginAsync(
        HttpContext httpContext,
        [FromForm] LoginInput loginInput,
        [FromForm] string? returnUrl,
        IAuthService authService)
    {
        if (string.IsNullOrWhiteSpace(loginInput.Email) || string.IsNullOrWhiteSpace(loginInput.Password))
        {
            return Results.LocalRedirect("/login?error=invalid");
        }

        var userId = await authService.LoginUserAsync(loginInput.Email, loginInput.Password);

        if (!userId.HasValue)
        {
            return Results.LocalRedirect("/login?error=invalid");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(ClaimTypes.Name, loginInput.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });

        var targetUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/persons" : returnUrl;

        if (!targetUrl.StartsWith("/"))
        {
            targetUrl = "/" + targetUrl;
        }

        return Results.LocalRedirect(targetUrl);
    }

    private static async Task<IResult> HandleLogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.LocalRedirect("/");
    }
}