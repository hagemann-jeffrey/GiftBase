namespace GiftBase.Shared;

public static class StringExtensions
{
    public static string? NormalizeOptional(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string NormalizeRequired(this string value) => value.Trim();
}
