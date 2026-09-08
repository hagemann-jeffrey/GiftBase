using System.Globalization;
using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Shared;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GiftBase.Features.Gifts
{
    public partial class GiftDialog(IGiftService giftService, UserActionHelper userActionHelper)
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;
        [Parameter]
        public GiftInput GiftInput { get; set; } = new();
        [Parameter, EditorRequired]
        public int PersonId { get; set; }
        [Parameter, EditorRequired]
        public string PersonName { get; set; } = string.Empty;
        [Parameter]
        public int? ExistingGiftId { get; set; }
        [Parameter]
        public IReadOnlyList<Occasion> Occasions { get; set; } = [];

        private bool HasDeletedOccasionSnapshot =>
            !GiftInput.OccasionId.HasValue && !string.IsNullOrWhiteSpace(GiftInput.OccasionLabel);

        private string DeletedOccasionText => $"{GiftInput.OccasionLabel} {GiftInput.OccasionYear}";

        private static CultureInfo GermanCulture => AppCulture.German;

        private string DialogTitle => ExistingGiftId.HasValue ? "Geschenkidee bearbeiten" : "Geschenkidee hinzufügen";

        private string DialogSubtitle => ExistingGiftId.HasValue
            ? $"Idee für {PersonName} bearbeiten"
            : $"Neue Idee für {PersonName} speichern";

        public void Cancel() => MudDialog.Cancel();

        public async Task Save()
        {
            await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
            {
                if (ExistingGiftId.HasValue)
                {
                    var updatedGift = await giftService.UpdateGiftAsync(ExistingGiftId.Value, userId, new GiftUpdateDto
                    {
                        Title = GiftInput.Title.NormalizeRequired(),
                        Note = GiftInput.Note.NormalizeOptional(),
                        Link = GiftInput.Link.NormalizeOptional(),
                        Price = GiftInput.Price,
                        Status = GiftInput.Status,
                        OccasionId = GiftInput.OccasionId
                    });

                    MudDialog.Close(DialogResult.Ok(updatedGift));
                }
                else
                {
                    var newGift = new GiftAddDto
                    {
                        Title = GiftInput.Title.NormalizeRequired(),
                        Note = GiftInput.Note.NormalizeOptional(),
                        Link = GiftInput.Link.NormalizeOptional(),
                        Price = GiftInput.Price,
                        Status = GiftInput.Status,
                        PersonId = PersonId,
                        OccasionId = GiftInput.OccasionId
                    };

                    var addedGift = await giftService.AddGiftAsync(newGift, userId);

                    MudDialog.Close(DialogResult.Ok(addedGift));
                }
            });
        }
    }
}