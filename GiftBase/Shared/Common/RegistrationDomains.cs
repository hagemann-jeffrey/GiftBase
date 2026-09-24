namespace GiftBase.Shared.Common;

public static class RegistrationDomains
{
    public static readonly string[] Allowed = ["iu-study.org", "iu.org"];

    public static bool IsAllowed(string? email) =>
        email is not null && Allowed.Any(domain => email.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase));
}
