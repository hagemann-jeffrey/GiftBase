using GiftBase.Core.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GiftBase.Features.Gifts.Components;

public partial class GiftListItem
{
    [Parameter, EditorRequired]
    public Core.Entities.Gift Gift { get; set; } = null!;
    [Parameter]
    public RenderFragment? HeaderActionContent { get; set; }

    private Color StatusColor => Gift.Status switch
    {
        GiftStatus.Idea => Color.Default,
        GiftStatus.Bought => Color.Info,
        GiftStatus.Wrapped => Color.Warning,
        GiftStatus.Given => Color.Success,
        _ => Color.Default
    };

    private string PriceText => $"{Gift.Price!.Value:N2} €";
}