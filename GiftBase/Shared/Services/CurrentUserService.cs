using System.Security.Claims;
using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace GiftBase.Shared.Services;

public class CurrentUserService(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserService
{
    public async Task<int?> GetCurrentUserIdAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();

        var user = authState.User;

        if (user.Identity?.IsAuthenticated ?? false)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }

        return null;
    }
}