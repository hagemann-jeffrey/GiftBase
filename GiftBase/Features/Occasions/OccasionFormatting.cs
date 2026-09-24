using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Shared.Common;
using MudBlazor;

namespace GiftBase.Features.Occasions;

public static class OccasionFormatting
{
    public static string GetIcon(this Occasion occasion) => occasion.Type switch
    {
        OccasionType.Birthday => Icons.Material.Filled.Cake,
        OccasionType.Christmas => Icons.Material.Filled.Park,
        _ => Icons.Material.Filled.Event
    };

    public static string FormatNextOccurrence(this Occasion occasion, DateOnly today) =>
        occasion.GetNextOccurrence(today).ToString("dd. MMMM yyyy", AppCulture.German);

    public static string FormatCountdown(this Occasion occasion, DateOnly today)
    {
        var daysUntilNextOccurrence = occasion.GetNextOccurrence(today).DayNumber - today.DayNumber;

        return daysUntilNextOccurrence switch
        {
            < 0 => "vergangen",
            0 => "heute",
            1 => "morgen",
            _ => $"in {daysUntilNextOccurrence} Tagen"
        };
    }
}
