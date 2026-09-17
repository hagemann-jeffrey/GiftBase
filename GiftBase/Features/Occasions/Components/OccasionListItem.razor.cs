using GiftBase.Features.Occasions;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Occasions.Components;

public partial class OccasionListItem
{
    [Parameter, EditorRequired]
    public Core.Entities.Occasion Occasion { get; set; } = null!;
    [Parameter]
    public int GiftCount { get; set; }
    [Parameter]
    public RenderFragment? HeaderActionContent { get; set; }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private string OccasionIcon => Occasion.GetIcon();

    private string NextOccurrenceText => Occasion.FormatNextOccurrence(Today);

    private string RecurrenceText => Occasion.IsRecurring ? "jährlich" : "einmalig";

    private string GiftCountText => GiftCount == 1 ? "1 Geschenkidee" : $"{GiftCount} Geschenkideen";

    private string CountdownText => Occasion.FormatCountdown(Today);
}
