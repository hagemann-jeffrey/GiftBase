using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Shared.Common;
using GiftBase.Shared.Services;
using MudBlazor;

namespace GiftBase.Features.Persons;

public partial class Overview(
    IPersonService PersonService,
    IGiftService GiftService,
    IOccasionService OccasionService,
    IDialogService DialogService,
    UserActionHelper userActionHelper)
{
    private List<Person> Persons = [];
    private Dictionary<int, int> GiftCounts = [];
    private Dictionary<int, Occasion> NextOccasions = [];
    private bool IsLoading = true;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    override protected async Task OnInitializedAsync()
    {
        IsLoading = true;

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var persons = await PersonService.GetPersonsAsync(userId);
            GiftCounts = await GiftService.GetGiftCountsByPersonAsync(userId);
            NextOccasions = await OccasionService.GetNextOccasionsByPersonAsync(userId);
            Persons = persons.SortByNextOccasion(NextOccasions, Today);
        });

        IsLoading = false;
    }

    private int GetGiftCount(int personId) => GiftCounts.TryGetValue(personId, out var giftCount) ? giftCount : 0;

    private Occasion? GetNextOccasion(int personId) => NextOccasions.GetValueOrDefault(personId);

    private async Task AddPersonAsync()
    {
        var dialog = await DialogService.ShowAsync<PersonDialog>(title: null, options: AppDialogOptions.Default);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Person addedPerson })
        {
            Persons.Add(addedPerson);
            GiftCounts[addedPerson.Id] = 0;
            Persons = Persons.SortByNextOccasion(NextOccasions, Today);
        }
    }
}