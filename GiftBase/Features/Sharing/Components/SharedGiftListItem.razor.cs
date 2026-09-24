using GiftBase.Core.Dtos.Sharing;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Sharing.Components;

public partial class SharedGiftListItem
{
    [Parameter, EditorRequired]
    public SharedGiftDto Gift { get; set; } = null!;

    private string PriceText => $"{Gift.Price!.Value:N2} €";
}
