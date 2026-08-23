using Microsoft.AspNetCore.Components;

namespace GiftBase.Shared.Components
{
    public partial class PageHeader
    {
        [Parameter, EditorRequired]
        public string Title { get; set; } = string.Empty;

        [Parameter]
        public string? Description { get; set; }

        [Parameter]
        public RenderFragment? ActionContent { get; set; }
    }
}