using System.Globalization;
using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Features.GiftSuggestions.Components;
using GiftBase.Shared;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Translations = GiftBase.Shared.Translations.Translations;

namespace GiftBase.Features.GiftSuggestions;

public partial class GiftSuggestionDialog(IGiftSuggestionService giftSuggestionService, IGiftService giftService, UserActionHelper userActionHelper)
{
    private enum DialogStep
    {
        Input,
        Loading,
        Results
    }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter, EditorRequired]
    public Person Person { get; set; } = null!;
    [Parameter]
    public IReadOnlyList<Occasion> Occasions { get; set; } = [];
    [Parameter]
    public IReadOnlyList<Gift> ExistingGifts { get; set; } = [];

    private GiftSuggestionInput GiftSuggestionInput { get; set; } = new();
    private DialogStep Step { get; set; } = DialogStep.Input;
    private List<GiftSuggestionRow> Suggestions { get; set; } = [];
    private string? ErrorMessage { get; set; }

    private static CultureInfo GermanCulture => AppCulture.German;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private string PersonName => $"{Person.FirstName} {Person.LastName}";

    private string DialogSubtitle => $"Passende Ideen für {PersonName} generieren";

    private int SelectedCount => Suggestions.Count(s => s.Selected);

    private string ExistingGiftTitles => string.Join(", ", ExistingGifts.Select(g => g.Title));

    private string RelationText => Translations.GetRelationDisplayText(Person.Relation);

    private int? Age => Person.GetAge(Today);

    private string SelectedOccasionText
    {
        get
        {
            var occasion = Occasions.SingleOrDefault(o => o.Id == GiftSuggestionInput.OccasionId);

            return occasion is null ? "–" : Translations.GetOccasionDisplayTitle(occasion);
        }
    }

    protected override void OnInitialized()
    {
        GiftSuggestionInput.OccasionId = Occasions.FirstOrDefault()?.Id;
    }

    public void Cancel() => MudDialog.Cancel();

    private async Task GenerateAsync()
    {
        ErrorMessage = null;
        Step = DialogStep.Loading;

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            try
            {
                var suggestions = await giftSuggestionService.GenerateAsync(new GiftSuggestionGenerateDto
                {
                    PersonId = Person.Id,
                    OccasionId = GiftSuggestionInput.OccasionId!.Value,
                    Budget = GiftSuggestionInput.Budget!.Value,
                    AdditionalHint = GiftSuggestionInput.AdditionalHint.NormalizeOptional()
                }, userId);

                Suggestions = [.. suggestions.Select(s => new GiftSuggestionRow(s))];
                Step = DialogStep.Results;
            }
            catch (ConflictException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (ExternalServiceException ex)
            {
                ErrorMessage = ex.Message;
            }
        });

        if (Step == DialogStep.Loading)
        {
            Step = DialogStep.Input;
        }
    }

    private async Task AdoptSelectedAsync()
    {
        var selectedRows = Suggestions.Where(s => s.Selected).ToList();

        if (selectedRows.Count == 0)
        {
            return;
        }

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var addedGifts = new List<Gift>();

            foreach (var row in selectedRows)
            {
                var suggestion = row.Suggestion;

                var addedGift = await giftService.AddGiftAsync(new GiftAddDto
                {
                    Title = suggestion.Title,
                    Note = suggestion.Reason,
                    Link = GiftSuggestionLinkBuilder.Build(suggestion.Type, suggestion.SearchTerm),
                    Price = suggestion.Price,
                    Status = GiftStatus.Idea,
                    PersonId = Person.Id,
                    OccasionId = GiftSuggestionInput.OccasionId
                }, userId);

                addedGifts.Add(addedGift);
            }

            MudDialog.Close(DialogResult.Ok(addedGifts));
        });
    }
}
