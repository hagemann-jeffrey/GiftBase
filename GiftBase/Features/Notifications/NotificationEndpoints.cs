using System.Security.Cryptography;
using System.Text;
using GiftBase.Core.Interfaces;
using GiftBase.Options;
using GiftBase.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GiftBase.Features.Notifications;

public static class NotificationEndpoints
{
    private const string SecretHeaderName = "X-GiftBase-Notification-Secret";

    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/notifications/dispatch", HandleDispatchAsync)
            .AllowAnonymous()
            .DisableAntiforgery()
            .WithMetadata(new SkipStatusCodePagesAttribute());

        return endpoints;
    }

    private static async Task<IResult> HandleDispatchAsync(
        HttpContext httpContext,
        IOptions<NotificationOptions> options,
        INotificationService notificationService,
        bool? dryRun)
    {
        var expectedSecret = options.Value.TriggerSecret;
        var providedSecret = httpContext.Request.Headers[SecretHeaderName].ToString();

        if (string.IsNullOrEmpty(expectedSecret) || !IsMatchingSecret(expectedSecret, providedSecret))
        {
            return Results.Unauthorized();
        }

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AppTimeZone.German));
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var isDryRun = dryRun ?? false;

        var result = await notificationService.DispatchAsync(today, baseUrl, isDryRun);

        return Results.Ok(result);
    }

    private static bool IsMatchingSecret(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);

        return expectedBytes.Length == providedBytes.Length && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
