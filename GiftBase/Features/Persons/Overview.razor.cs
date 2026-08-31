using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Shared.Services;
using MudBlazor;

namespace GiftBase.Features.Persons;

public partial class Overview(IPersonService PersonService, IGiftService GiftService, IDialogService DialogService, UserActionHelper userActionHelper)
{
    private List<Person> Persons = [];
    private Dictionary<int, int> GiftCounts = [];
    private bool IsLoading = true;

    override protected async Task OnInitializedAsync()
    {
        IsLoading = true;

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            Persons = await PersonService.GetPersonsAsync(userId);
            GiftCounts = await GiftService.GetGiftCountsByPersonAsync(userId);
        });

        IsLoading = false;
    }

    private int GetGiftCount(int personId) => GiftCounts.TryGetValue(personId, out var giftCount) ? giftCount : 0;

    private async Task AddPersonAsync()
    {
        var dialogOptions = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<PersonDialog>(title: null, options: dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Person addedPerson })
        {
            Persons.Add(addedPerson);
            GiftCounts[addedPerson.Id] = 0;
        }
    }
}