using System.Globalization;

namespace GiftBase.Shared.Common;

public static class AppCulture
{
    public const string Name = "de-DE";

    public static readonly CultureInfo German = CultureInfo.GetCultureInfo(Name);
}