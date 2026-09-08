using System.Globalization;
using GiftBase.Core.Dtos;
using GiftBase.Core.Enums;
using GiftBase.Core.Interfaces;
using GiftBase.Shared;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Translations = GiftBase.Shared.Translations.Translations;

namespace GiftBase.Features.Occasions
{
    public partial class OccasionDialog(IOccasionService occasionService, UserActionHelper userActionHelper)
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;
        [Parameter]
        public OccasionInput OccasionInput { get; set; } = new();
        [Parameter, EditorRequired]
        public int PersonId { get; set; }
        [Parameter, EditorRequired]
        public string PersonName { get; set; } = string.Empty;
        [Parameter]
        public DateOnly? PersonDateOfBirth { get; set; }
        [Parameter]
        public IReadOnlyCollection<OccasionType> ExistingTypes { get; set; } = [];
        [Parameter]
        public int? ExistingOccasionId { get; set; }

        private static CultureInfo GermanCulture => AppCulture.German;

        private bool IsCustom => OccasionInput.Type == OccasionType.Custom;

        private string DialogTitle => ExistingOccasionId.HasValue ? "Anlass bearbeiten" : "Anlass hinzufügen";

        private string DialogSubtitle => ExistingOccasionId.HasValue
            ? $"Anlass für {PersonName} bearbeiten"
            : $"Neuen Anlass für {PersonName} eintragen";

        private string? FixedValueHint => IsCustom
            ? null
            : "Datum und Wiederholung sind bei diesem Anlass fest vorgegeben.";

        private string? GetDisabledReason(OccasionType occasionType)
        {
            if (occasionType == OccasionType.Custom)
            {
                return null;
            }

            if (ExistingTypes.Contains(occasionType))
            {
                return "bereits angelegt";
            }

            return occasionType == OccasionType.Birthday && !PersonDateOfBirth.HasValue
                ? "Geburtsdatum fehlt"
                : null;
        }

        private string GetTypeLabel(OccasionType occasionType)
        {
            var label = Translations.GetOccasionTypeDisplayText(occasionType);
            var disabledReason = GetDisabledReason(occasionType);

            return disabledReason is null ? label : $"{label} · {disabledReason}";
        }

        private bool IsTypeDisabled(OccasionType occasionType) => GetDisabledReason(occasionType) is not null;

        private void OnTypeChanged()
        {
            if (IsCustom)
            {
                OccasionInput.Date = null;
                OccasionInput.IsRecurring = false;
                return;
            }

            OccasionInput.Title = null;
            OccasionInput.IsRecurring = true;
            OccasionInput.Date = OccasionInput.Type == OccasionType.Birthday
                ? PersonDateOfBirth?.ToDateTime(TimeOnly.MinValue)
                : new DateTime(DateTime.Today.Year, 12, 24);
        }

        public void Cancel() => MudDialog.Cancel();

        public async Task Save()
        {
            await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
            {
                if (ExistingOccasionId.HasValue)
                {
                    var updatedOccasion = await occasionService.UpdateOccasionAsync(ExistingOccasionId.Value, userId, new OccasionUpdateDto
                    {
                        Title = OccasionInput.Title.NormalizeOptional(),
                        Date = DateOnly.FromDateTime(OccasionInput.Date!.Value),
                        IsRecurring = OccasionInput.IsRecurring
                    });

                    MudDialog.Close(DialogResult.Ok(updatedOccasion));
                }
                else
                {
                    var addedOccasion = await occasionService.AddOccasionAsync(new OccasionAddDto
                    {
                        Type = OccasionInput.Type,
                        Title = OccasionInput.Title.NormalizeOptional(),
                        Date = DateOnly.FromDateTime(OccasionInput.Date!.Value),
                        IsRecurring = OccasionInput.IsRecurring,
                        PersonId = PersonId
                    }, userId);

                    MudDialog.Close(DialogResult.Ok(addedOccasion));
                }
            });
        }
    }
}
