using GiftBase.Core.Dtos.Sharing;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Sharing;

public partial class SharedGiftList(IShareLinkService shareLinkService)
{
    [Parameter]
    public string Token { get; set; } = string.Empty;

    private SharedGiftListDto? GiftList;
    private bool IsLoading = true;
    private string? _loadedToken;

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedToken == Token)
        {
            return;
        }

        _loadedToken = Token;
        IsLoading = true;
        GiftList = null;

        try
        {
            GiftList = await shareLinkService.GetSharedGiftListAsync(Token);
        }
        catch (NotFoundException)
        {
            GiftList = null;
        }

        IsLoading = false;
    }

    private string HeaderTitle => GiftList!.OccasionTitle is null
        ? $"Geschenkideen für {GiftList.RecipientFirstName}"
        : $"{GiftList.OccasionTitle} für {GiftList.RecipientFirstName}";
}
