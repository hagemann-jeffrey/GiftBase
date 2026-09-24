using GiftBase.Core.Entities;
using GiftBase.Core.Enums;

namespace GiftBase.Shared.Common;

public static class Translations
{
    public static string GetRelationDisplayText(Relation relation) => relation switch
    {
        Relation.Friend => "Freund",
        Relation.Family => "Familie",
        Relation.Colleague => "Kollege",
        Relation.Partner => "Partner",
        Relation.Neighbor => "Nachbar",
        Relation.Other => "Sonstige",
        _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
    };

    public static string GetGiftStatusDisplayText(GiftStatus giftStatus) => giftStatus switch
    {
        GiftStatus.Idea => "Idee",
        GiftStatus.Bought => "Gekauft",
        GiftStatus.Wrapped => "Eingepackt",
        GiftStatus.Given => "Verschenkt",
        _ => throw new ArgumentOutOfRangeException(nameof(giftStatus), giftStatus, null)
    };

    public static string GetOccasionTypeDisplayText(OccasionType occasionType) => occasionType switch
    {
        OccasionType.Birthday => "Geburtstag",
        OccasionType.Christmas => "Weihnachten",
        OccasionType.Custom => "Benutzerdefiniert",
        _ => throw new ArgumentOutOfRangeException(nameof(occasionType), occasionType, null)
    };

    public static string GetOccasionDisplayTitle(Occasion occasion) =>
        occasion.Type == OccasionType.Custom ? occasion.Title ?? string.Empty : GetOccasionTypeDisplayText(occasion.Type);

    public static string GetGiftSuggestionTypeDisplayText(GiftSuggestionType giftSuggestionType) => giftSuggestionType switch
    {
        GiftSuggestionType.Product => "Produkt",
        GiftSuggestionType.Service => "Dienstleistung",
        GiftSuggestionType.Money => "Geld",
        _ => throw new ArgumentOutOfRangeException(nameof(giftSuggestionType), giftSuggestionType, null)
    };
}