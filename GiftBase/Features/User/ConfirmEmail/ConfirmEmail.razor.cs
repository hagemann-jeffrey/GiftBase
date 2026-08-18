using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.User.ConfirmEmail;

public partial class ConfirmEmail(IAuthService authService)
{
    [Parameter]
    public required string Token { get; set; }

    private bool IsLoading = true;
    private bool IsSuccess = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsSuccess = await authService.ConfirmEmailAsync(Token);
            IsLoading = false;

            StateHasChanged();
        }
    }
}