using System.Globalization;
using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Shared;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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

        private string? SelectedImageDataUrl;
        private string? ImageError;

        private string? ExistingImageUrl =>
            ExistingGiftId.HasValue && GiftInput.ImageVersion.HasValue
                ? GiftImageLinkBuilder.Build(ExistingGiftId.Value, GiftInput.ImageVersion.Value)
                : null;

        private string? PreviewImageUrl => SelectedImageDataUrl ?? ExistingImageUrl;

        private static CultureInfo GermanCulture => AppCulture.German;

        private string DialogTitle => ExistingGiftId.HasValue ? "Geschenkidee bearbeiten" : "Geschenkidee hinzufügen";

        private string DialogSubtitle => ExistingGiftId.HasValue
            ? $"Idee für {PersonName} bearbeiten"
            : $"Neue Idee für {PersonName} speichern";

        public void Cancel() => MudDialog.Cancel();

        private async Task OnImageSelectedAsync(IBrowserFile? file)
        {
            if (file is null)
            {
                return;
            }

            if (file.Size > GiftImage.MaxContentLength)
            {
                ImageError = "Das Bild darf maximal 5 MB groß sein.";
                return;
            }

            if (!GiftImage.IsSupportedContentType(file.ContentType))
            {
                ImageError = "Nur JPG- und PNG-Bilder werden unterstützt.";
                return;
            }

            await using var stream = file.OpenReadStream(GiftImage.MaxContentLength);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            ImageError = null;
            GiftInput.ImageContent = memoryStream.ToArray();
            GiftInput.ImageContentType = file.ContentType;
            SelectedImageDataUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(GiftInput.ImageContent)}";
        }

        private void RemoveImage()
        {
            GiftInput.ImageContent = null;
            GiftInput.ImageContentType = null;
            GiftInput.ImageVersion = null;
            SelectedImageDataUrl = null;
            ImageError = null;
        }

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
                        OccasionId = GiftInput.OccasionId,
                        ImageContent = GiftInput.ImageContent,
                        ImageContentType = GiftInput.ImageContentType,
                        ImageVersion = GiftInput.ImageVersion
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
                        PersonId = PersonId,
                        OccasionId = GiftInput.OccasionId,
                        ImageContent = GiftInput.ImageContent,
                        ImageContentType = GiftInput.ImageContentType
                    };

                    var addedGift = await giftService.AddGiftAsync(newGift, userId);

                    MudDialog.Close(DialogResult.Ok(addedGift));
                }
            });
        }
    }
}