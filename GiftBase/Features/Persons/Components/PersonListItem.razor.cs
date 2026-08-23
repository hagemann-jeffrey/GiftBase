using Microsoft.AspNetCore.Components;

namespace GiftBase.Features.Persons.Components;

public partial class PersonListItem
{
    [Parameter, EditorRequired]
    public Core.Entities.Person Person { get; set; } = null!;
    [Parameter]
    public RenderFragment? HeaderActionContent { get; set; }

    private string GetInitials()
    {
        var firstInitial = string.IsNullOrWhiteSpace(Person.FirstName) ? "" : Person.FirstName[0].ToString();
        var lastInitial = string.IsNullOrWhiteSpace(Person.LastName) ? "" : Person.LastName[0].ToString();
        return $"{firstInitial}{lastInitial}".ToUpper();
    }
}