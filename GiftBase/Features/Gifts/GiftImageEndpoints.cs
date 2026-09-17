using System.Security.Claims;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GiftBase.Features.Gifts;

public static class GiftImageEndpoints
{
    public static IEndpointRouteBuilder MapGiftImageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/gifts/{giftId:int}/image", HandleGetGiftImageAsync)
            .RequireAuthorization()
            .WithMetadata(new SkipStatusCodePagesAttribute());

        return endpoints;
    }

    private static async Task<IResult> HandleGetGiftImageAsync(
        HttpContext httpContext,
        int giftId,
        IGiftService giftService)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return Results.NotFound();
        }

        try
        {
            var giftImage = await giftService.GetGiftImageAsync(giftId, currentUserId);

            httpContext.Response.Headers.CacheControl = "private, max-age=31536000, immutable";
            httpContext.Response.Headers.XContentTypeOptions = "nosniff";

            return Results.File(giftImage.Content, giftImage.ContentType);
        }
        catch (NotFoundException)
        {
            return Results.NotFound();
        }
    }
}
