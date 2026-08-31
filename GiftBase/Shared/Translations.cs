using GiftBase.Core.Enums;

namespace GiftBase.Shared.Translations;

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
}