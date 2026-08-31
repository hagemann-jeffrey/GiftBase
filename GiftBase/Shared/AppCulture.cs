using System.Globalization;

namespace GiftBase.Shared;

public static class AppCulture
{
    public const string Name = "de-DE";

    public static readonly CultureInfo German = CultureInfo.GetCultureInfo(Name);
}