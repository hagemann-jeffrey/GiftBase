using System.Globalization;
using GiftBase.Core.Dtos;
using GiftBase.Core.Interfaces;
using GiftBase.Shared;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GiftBase.Features.Persons
{
    public partial class PersonDialog(IPersonService personService, UserActionHelper userActionHelper)
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;
        [Parameter]
        public PersonInput PersonInput { get; set; } = new();
        [Parameter]
        public int? ExistingPersonId { get; set; }

        private static CultureInfo GermanCulture => AppCulture.German;

        private string DialogTitle => ExistingPersonId.HasValue ? "Person bearbeiten" : "Person hinzufügen";

        private string DialogSubtitle => ExistingPersonId.HasValue
            ? "Stammdaten dieser Person bearbeiten"
            : "Neue Person für deine Geschenkideen anlegen";

        public void Cancel() => MudDialog.Cancel();

        public async Task Save()
        {
            await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
            {
                if (ExistingPersonId.HasValue)
                {
                    var updatedPerson = await personService.UpdatePersonAsync(ExistingPersonId.Value, userId, new Core.Dtos.PersonUpdateDto
                    {
                        FirstName = PersonInput.FirstName.NormalizeRequired(),
                        LastName = PersonInput.LastName.NormalizeRequired(),
                        DateOfBirth = PersonInput.DateOfBirth.HasValue ? DateOnly.FromDateTime(PersonInput.DateOfBirth.Value) : null,
                        Relation = PersonInput.Relation
                    });

                    MudDialog.Close(DialogResult.Ok(updatedPerson));
                }
                else
                {
                    var newPerson = new PersonAddDto
                    {
                        FirstName = PersonInput.FirstName.NormalizeRequired(),
                        LastName = PersonInput.LastName.NormalizeRequired(),
                        DateOfBirth = PersonInput.DateOfBirth.HasValue ? DateOnly.FromDateTime(PersonInput.DateOfBirth.Value) : null,
                        Relation = PersonInput.Relation,
                        UserId = userId
                    };

                    var addedPerson = await personService.AddPersonAsync(newPerson);

                    MudDialog.Close(DialogResult.Ok(addedPerson));
                }
            });
        }
    }
}