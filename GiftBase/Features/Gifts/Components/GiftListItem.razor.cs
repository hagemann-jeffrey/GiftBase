using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Gifts.Components;

public partial class GiftListItem
{
    [Parameter, EditorRequired]
    public Core.Entities.Gift Gift { get; set; } = null!;
    [Parameter]
    public RenderFragment? HeaderActionContent { get; set; }
    [Parameter]
    public string? OccasionText { get; set; }

    private string PriceText => $"{Gift.Price!.Value:N2} €";
}