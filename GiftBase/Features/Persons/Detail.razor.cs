using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Features.Gifts;
using GiftBase.Shared.Components;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GiftBase.Features.Persons;

public partial class Detail(
    IPersonService PersonService,
    IGiftService GiftService,
    IDialogService DialogService,
    UserActionHelper userActionHelper,
    NavigationManager navigationManager)
{
    [Parameter]
    public int PersonId { get; set; }

    private Person? Person;

    private List<Gift> Gifts = [];
    private bool IsLoading = true;
    private int? _loadedPersonId;

    override protected async Task OnParametersSetAsync()
    {
        if (_loadedPersonId == PersonId)
        {
            return;
        }

        _loadedPersonId = PersonId;
        IsLoading = true;
        Person = null;
        Gifts = [];

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            Person = await PersonService.GetPersonAsync(PersonId, userId);
            Gifts = await GiftService.GetGiftsAsync(PersonId, userId);
        });

        IsLoading = false;
    }

    private string PersonFullName => Person is null ? string.Empty : $"{Person.FirstName} {Person.LastName}";

    private static DialogOptions DefaultDialogOptions => new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
        CloseOnEscapeKey = true
    };

    private async Task AddGiftAsync()
    {
        var parameters = new DialogParameters<GiftDialog>
        {
            { x => x.PersonId, PersonId },
            { x => x.PersonName, PersonFullName }
        };

        var dialog = await DialogService.ShowAsync<GiftDialog>(null, parameters, DefaultDialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Gift addedGift })
        {
            Gifts.Add(addedGift);
        }
    }

    private async Task UpdateGiftAsync(Gift gift)
    {
        var parameters = new DialogParameters<GiftDialog>
        {
            { x => x.GiftInput, new GiftInput
            {
                Title = gift.Title,
                Note = gift.Note,
                Link = gift.Link,
                Price = gift.Price,
                Status = gift.Status
            }
            },
            { x => x.PersonId, PersonId },
            { x => x.PersonName, PersonFullName },
            { x => x.ExistingGiftId, gift.Id }
        };

        var dialog = await DialogService.ShowAsync<GiftDialog>(null, parameters, DefaultDialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Gift updatedGift })
        {
            var index = Gifts.FindIndex(g => g.Id == updatedGift.Id);
            if (index != -1)
            {
                Gifts[index] = updatedGift;
            }
        }
    }

    private async Task DeleteGiftAsync(Gift gift)
    {
        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var parameters = new DialogParameters<DeleteDialog>
            {
                { x => x.Message, $"Möchten Sie die Geschenkidee \"{gift.Title}\" wirklich löschen?" }
            };

            var dialog = await DialogService.ShowAsync<DeleteDialog>(null, parameters, DefaultDialogOptions);

            var result = await dialog.Result;

            if (result is { Canceled: false, Data: true })
            {
                await GiftService.DeleteGiftAsync(gift.Id, userId);
                Gifts.Remove(gift);
            }
        });
    }

    private async Task UpdatePersonAsync()
    {
        if (Person is null)
        {
            return;
        }

        var parameters = new DialogParameters<PersonDialog>
        {
            { x => x.PersonInput, new PersonInput
            {
                FirstName = Person.FirstName,
                LastName = Person.LastName,
                DateOfBirth = Person.DateOfBirth.HasValue ? Person.DateOfBirth.Value.ToDateTime(new TimeOnly(0, 0)) : null,
                Relation = Person.Relation
            }
            },
            { x => x.ExistingPersonId, Person.Id }
        };

        var dialog = await DialogService.ShowAsync<PersonDialog>(null, parameters, DefaultDialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Person updatedPerson })
        {
            Person = updatedPerson;
        }
    }

    private async Task DeletePersonAsync()
    {
        if (Person is null)
        {
            return;
        }

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var parameters = new DialogParameters<DeleteDialog>
            {
                { x => x.Message, $"Möchten Sie {Person.FirstName} {Person.LastName} wirklich löschen?" }
            };

            var dialog = await DialogService.ShowAsync<DeleteDialog>(null, parameters, DefaultDialogOptions);

            var result = await dialog.Result;

            if (result is { Canceled: false, Data: true })
            {
                await PersonService.DeletePersonAsync(Person.Id, userId);
                navigationManager.NavigateTo("/persons");
            }
        });
    }
}