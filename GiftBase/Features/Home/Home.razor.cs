using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Home;

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
