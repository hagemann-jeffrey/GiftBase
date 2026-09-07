using GiftBase.Core.Enums;
using GiftBase.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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

    private string OccasionIcon => Occasion.Type switch
    {
        OccasionType.Birthday => Icons.Material.Filled.Cake,
        OccasionType.Christmas => Icons.Material.Filled.Park,
        _ => Icons.Material.Filled.Event
    };

    private string NextOccurrenceText =>
        Occasion.GetNextOccurrence(Today).ToString("dd. MMMM yyyy", AppCulture.German);

    private string RecurrenceText => Occasion.IsRecurring ? "jährlich" : "einmalig";

    private Color RecurrenceColor => Occasion.IsRecurring ? Color.Primary : Color.Default;

    private string GiftCountText => GiftCount == 1 ? "1 Geschenkidee" : $"{GiftCount} Geschenkideen";

    private string CountdownText
    {
        get
        {
            var daysUntilNextOccurrence = Occasion.GetNextOccurrence(Today).DayNumber - Today.DayNumber;

            return daysUntilNextOccurrence switch
            {
                < 0 => "vergangen",
                0 => "heute",
                1 => "morgen",
                _ => $"in {daysUntilNextOccurrence} Tagen"
            };
        }
    }
}
