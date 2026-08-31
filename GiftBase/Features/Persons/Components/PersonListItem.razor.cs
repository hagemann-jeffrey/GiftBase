using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Persons.Components;

public partial class PersonListItem
{
    [Parameter, EditorRequired]
    public Core.Entities.Person Person { get; set; } = null!;
    [Parameter]
    public int GiftCount { get; set; }

    private string GiftCountText => GiftCount == 1 ? "1 Geschenkidee" : $"{GiftCount} Geschenkideen";

    private string GetInitials()
    {
        var firstInitial = string.IsNullOrWhiteSpace(Person.FirstName) ? "" : Person.FirstName[0].ToString();
        var lastInitial = string.IsNullOrWhiteSpace(Person.LastName) ? "" : Person.LastName[0].ToString();
        return $"{firstInitial}{lastInitial}".ToUpper();
    }
}