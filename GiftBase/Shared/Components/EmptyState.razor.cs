using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GiftBase.Shared.Components;

public partial class EmptyState
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.Inbox;

    [Parameter]
    public RenderFragment? ActionContent { get; set; }
}
