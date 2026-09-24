using GiftBase.Shared.Common;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.GiftSuggestions.Components;

public partial class GiftSuggestionListItem
{
    [Parameter, EditorRequired]
    public GiftSuggestionRow Row { get; set; } = null!;
    [Parameter]
    public EventCallback OnSelectionChanged { get; set; }

    private string TypeText => Translations.GetGiftSuggestionTypeDisplayText(Row.Suggestion.Type);
    private string PriceText => $"{Row.Suggestion.Price:N2} €";
    private string? Link => GiftSuggestionLinkBuilder.Build(Row.Suggestion.Type, Row.Suggestion.SearchTerm);

    private async Task HandleSelectedChangedAsync(bool value)
    {
        Row.Selected = value;
        await OnSelectionChanged.InvokeAsync();
    }
}
