using GiftBase.Core.Interfaces;

namespace GiftBase.Features.Persons;

public partial class Overview(IPersonService PersonService, ICurrentUserService CurrentUserService)
{
    private List<Core.Entities.Person> Persons = [];
    private bool IsLoading = false;

    override protected async Task OnInitializedAsync()
    {
        IsLoading = true;

        var userId = await CurrentUserService.GetCurrentUserIdAsync();

        if (userId.HasValue)
        {
            Persons = await PersonService.GetPersonsAsync(userId.Value);
        }

        IsLoading = false;
    }

}