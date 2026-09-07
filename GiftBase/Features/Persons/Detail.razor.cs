using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Interfaces;
using GiftBase.Features.Gifts;
using GiftBase.Features.Occasions;
using GiftBase.Shared;
using GiftBase.Shared.Components;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Translations = GiftBase.Shared.Translations.Translations;

namespace GiftBase.Features.Persons;

public partial class Detail(
    IPersonService PersonService,
    IGiftService GiftService,
    IOccasionService OccasionService,
    IDialogService DialogService,
    UserActionHelper userActionHelper,
    NavigationManager navigationManager)
{
    [Parameter]
    public int PersonId { get; set; }

    private Person? Person;

    private List<Gift> Gifts = [];
    private List<Occasion> Occasions = [];
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
        Occasions = [];

        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            Person = await PersonService.GetPersonAsync(PersonId, userId);
            Occasions = await OccasionService.GetOccasionsAsync(PersonId, userId);
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

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private string NextOccasionText
    {
        get
        {
            var nextOccasion = Occasions.FirstOrDefault(o => !o.IsPast(Today));

            if (nextOccasion is null)
            {
                return "Kein Anlass hinterlegt";
            }

            var nextOccurrence = nextOccasion.GetNextOccurrence(Today);
            var daysUntilNextOccurrence = nextOccurrence.DayNumber - Today.DayNumber;
            var countdown = daysUntilNextOccurrence switch
            {
                0 => "heute",
                1 => "morgen",
                _ => $"in {daysUntilNextOccurrence} Tagen"
            };

            return $"{Translations.GetOccasionDisplayTitle(nextOccasion)} · "
                + $"{nextOccurrence.ToString("dd. MMMM yyyy", AppCulture.German)} · {countdown}";
        }
    }

    private int GetGiftCountForOccasion(int occasionId) => Gifts.Count(g => g.OccasionId == occasionId);

    private async Task AddOccasionAsync()
    {
        if (Person is null)
        {
            return;
        }

        var parameters = new DialogParameters<OccasionDialog>
        {
            { x => x.PersonId, PersonId },
            { x => x.PersonName, PersonFullName },
            { x => x.PersonDateOfBirth, Person.DateOfBirth },
            { x => x.ExistingTypes, ExistingOccasionTypes }
        };

        var dialog = await DialogService.ShowAsync<OccasionDialog>(null, parameters, DefaultDialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Occasion addedOccasion })
        {
            Occasions.Add(addedOccasion);
            Occasions = Occasions.SortByNextOccurrence(Today);
        }
    }

    private async Task UpdateOccasionAsync(Occasion occasion)
    {
        if (Person is null)
        {
            return;
        }

        var parameters = new DialogParameters<OccasionDialog>
        {
            { x => x.OccasionInput, new OccasionInput
            {
                Type = occasion.Type,
                Title = occasion.Title,
                Date = occasion.Date.ToDateTime(TimeOnly.MinValue),
                IsRecurring = occasion.IsRecurring
            }
            },
            { x => x.PersonId, PersonId },
            { x => x.PersonName, PersonFullName },
            { x => x.PersonDateOfBirth, Person.DateOfBirth },
            { x => x.ExistingTypes, ExistingOccasionTypes },
            { x => x.ExistingOccasionId, occasion.Id }
        };

        var dialog = await DialogService.ShowAsync<OccasionDialog>(null, parameters, DefaultDialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: Occasion updatedOccasion })
        {
            var index = Occasions.FindIndex(o => o.Id == updatedOccasion.Id);

            if (index != -1)
            {
                Occasions[index] = updatedOccasion;
            }

            Occasions = Occasions.SortByNextOccurrence(Today);
        }
    }

    private async Task DeleteOccasionAsync(Occasion occasion)
    {
        await userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            var occasionTitle = Translations.GetOccasionDisplayTitle(occasion);
            var parameters = new DialogParameters<DeleteDialog>
            {
                { x => x.Message, $"Möchten Sie den Anlass \"{occasionTitle}\" wirklich löschen? "
                    + "Zugeordnete Geschenkideen bleiben erhalten." }
            };

            var dialog = await DialogService.ShowAsync<DeleteDialog>(null, parameters, DefaultDialogOptions);

            var result = await dialog.Result;

            if (result is { Canceled: false, Data: true })
            {
                await OccasionService.DeleteOccasionAsync(occasion.Id, userId);
                Occasions.Remove(occasion);
                Gifts = await GiftService.GetGiftsAsync(PersonId, userId);
            }
        });
    }

    private List<OccasionType> ExistingOccasionTypes => [.. Occasions.Select(o => o.Type)];
}