using GiftBase.Core.Enums;

namespace GiftBase.Features.GiftSuggestions;

public static class GiftSuggestionLinkBuilder
{
    public static string? Build(GiftSuggestionType type, string searchTerm) => type switch
    {
        GiftSuggestionType.Product => $"https://www.amazon.de/s?k={Uri.EscapeDataString(searchTerm)}",
        GiftSuggestionType.Service => $"https://www.google.com/search?q={Uri.EscapeDataString(searchTerm)}",
        GiftSuggestionType.Money => null,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
