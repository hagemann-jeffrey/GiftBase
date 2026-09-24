using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Shared.Common;
using GiftBase.Shared.Components;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GiftBase.Features.Sharing;

public partial class ShareDialog(
    IShareLinkService shareLinkService,
    IDialogService dialogService,
    UserActionHelper userActionHelper,
    NavigationManager navigationManager,
    IJSRuntime jsRuntime,
    ISnackbar snackbar)
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter, EditorRequired]
    public int PersonId { get; set; }

    [Parameter, EditorRequired]
    public string PersonName { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<Occasion> Occasions { get; set; } = [];

    private List<ShareLink> ShareLinks = [];
    private int? SelectedOccasionId;
    private bool IsLoading = true;

    private string DialogSubtitle => $"Geschenkideen für {PersonName} teilen";

    protected override async Task OnInitializedAsync()
    {
        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            ShareLinks = await shareLinkService.GetShareLinksAsync(PersonId, userId);
        });

        IsLoading = false;
    }

    private static bool IsExpired(ShareLink shareLink) => shareLink.IsExpired(DateTime.UtcNow);

    private static string GetValidityText(ShareLink shareLink) => IsExpired(shareLink)
        ? "Abgelaufen"
        : $"Gültig bis {shareLink.ExpiresAt.ToLocalTime():dd.MM.yyyy}";

    private string BuildShareUrl(ShareLink shareLink) =>
        navigationManager.ToAbsoluteUri($"share/{shareLink.Token}").ToString();

    private string GetScopeText(ShareLink shareLink)
    {
        if (!shareLink.OccasionId.HasValue)
        {
            return "Alle Geschenkideen";
        }

        var occasion = Occasions.SingleOrDefault(o => o.Id == shareLink.OccasionId.Value);

        return occasion is null ? "Gelöschter Anlass" : Translations.GetOccasionDisplayTitle(occasion);
    }

    private async Task CreateShareLinkAsync()
    {
        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var shareLink = await shareLinkService.AddShareLinkAsync(
                new ShareLinkAddDto { PersonId = PersonId, OccasionId = SelectedOccasionId }, userId);

            ShareLinks.Add(shareLink);
            snackbar.Add("Link wurde erstellt.", Severity.Success);
        });
    }

    private async Task DeleteShareLinkAsync(ShareLink shareLink)
    {
        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var parameters = new DialogParameters<DeleteDialog>
            {
                { x => x.Message, "Möchten Sie diesen Link wirklich löschen? Er kann danach nicht mehr geöffnet werden." }
            };

            var dialog = await dialogService.ShowAsync<DeleteDialog>(null, parameters);
            var result = await dialog.Result;

            if (result is { Canceled: false, Data: true })
            {
                await shareLinkService.DeleteShareLinkAsync(shareLink.Id, userId);
                ShareLinks.Remove(shareLink);
            }
        });
    }

    private async Task CopyShareUrlAsync(ShareLink shareLink)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", BuildShareUrl(shareLink));
            snackbar.Add("Link wurde in die Zwischenablage kopiert.", Severity.Success);
        }
        catch (JSException)
        {
            snackbar.Add("Der Link konnte nicht kopiert werden. Bitte markiere ihn im Feld und kopiere ihn manuell.",
                Severity.Warning);
        }
    }

    public void Close() => MudDialog.Close();
}
