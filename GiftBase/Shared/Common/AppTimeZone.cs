namespace GiftBase.Shared.Common;

public static class AppTimeZone
{
    public const string Id = "Europe/Berlin";

    public static readonly TimeZoneInfo German = TimeZoneInfo.FindSystemTimeZoneById(Id);
}
