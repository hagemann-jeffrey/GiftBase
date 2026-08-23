using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Shared.Components;
using GiftBase.Shared.Services;
using MudBlazor;

namespace GiftBase.Features.Persons;

public partial class Overview(IPersonService PersonService, IDialogService DialogService, UserActionHelper userActionHelper)
{
    private List<Person> Persons = [];
    private bool IsLoading = true;

    override protected async Task OnInitializedAsync()
    {
        IsLoading = true;

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            Persons = await PersonService.GetPersonsAsync(userId);
        });

        IsLoading = false;
    }

    private async Task AddPersonAsync()
    {
        var dialogOptions = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<PersonDialog>("Person hinzufügen", dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Person addedPerson })
        {
            Persons.Add(addedPerson);
        }
    }

    private async Task UpdatePersonAsync(Person person)
    {
        var dialogOptions = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        var parameters = new DialogParameters<PersonDialog>
        {
            { x => x.PersonInput, new PersonInput
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToDateTime(new TimeOnly(0, 0)) : null,
                Relation = person.Relation
            }
            },
            { x => x.ExistingPersonId, person.Id }
        };

        var dialog = await DialogService.ShowAsync<PersonDialog>("Person bearbeiten", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Person updatedPerson })
        {
            var index = Persons.FindIndex(p => p.Id == updatedPerson.Id);
            if (index != -1)
            {
                Persons[index] = updatedPerson;
            }
        }
    }

    private async Task DeletePersonAsync(Person person)
    {
        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var dialogOptions = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseOnEscapeKey = true
            };

            var parameters = new DialogParameters<DeleteDialog>
            {
                { x => x.Message, $"Möchten Sie {person.FirstName} {person.LastName} wirklich löschen?" }
            };

            var dialog = await DialogService.ShowAsync<DeleteDialog>(null, parameters, dialogOptions);

            var result = await dialog.Result;

            if (result is { Canceled: false, Data: true })
            {
                await PersonService.DeletePersonAsync(person.Id, userId);
                Persons.Remove(person);
            }
        });
    }
}