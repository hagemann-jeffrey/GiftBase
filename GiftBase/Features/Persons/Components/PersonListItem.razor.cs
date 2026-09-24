using GiftBase.Core.Entities;
using GiftBase.Features.Occasions;
using GiftBase.Shared.Common;
using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Persons.Components;

public partial class PersonListItem
{
    [Parameter, EditorRequired]
    public Person Person { get; set; } = null!;
    [Parameter]
    public int GiftCount { get; set; }
    [Parameter]
    public Occasion? NextOccasion { get; set; }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private string GiftCountText => GiftCount == 1 ? "1 Geschenkidee" : $"{GiftCount} Geschenkideen";

    private string NextOccasionIcon => NextOccasion?.GetIcon() ?? string.Empty;

    private string NextOccasionTitle => NextOccasion is null ? string.Empty : Translations.GetOccasionDisplayTitle(NextOccasion);

    private string NextOccasionDate => NextOccasion?.FormatNextOccurrence(Today) ?? string.Empty;

    private string NextOccasionCountdown => NextOccasion?.FormatCountdown(Today) ?? string.Empty;

    private string GetInitials()
    {
        var firstInitial = string.IsNullOrWhiteSpace(Person.FirstName) ? "" : Person.FirstName[0].ToString();
        var lastInitial = string.IsNullOrWhiteSpace(Person.LastName) ? "" : Person.LastName[0].ToString();
        return $"{firstInitial}{lastInitial}".ToUpper();
    }
}