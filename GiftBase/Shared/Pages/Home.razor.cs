using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Shared.Pages;

public partial class Home(ICurrentUserService currentUserService, NavigationManager navigationManager)
{
    protected override async Task OnInitializedAsync()
    {
        if (await currentUserService.GetCurrentUserIdAsync() is not null)
        {
            navigationManager.NavigateTo("/persons", replace: true);
        }
    }
}
