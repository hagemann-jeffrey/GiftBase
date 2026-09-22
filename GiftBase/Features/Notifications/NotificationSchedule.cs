using System.Globalization;

namespace GiftBase.Features.Notifications;

public static class NotificationSchedule
{
    public static bool IsMonthlyWindow(DateOnly today) => today.Day <= 7;

    public static DateOnly GetUpcomingMonth(DateOnly today) => new DateOnly(today.Year, today.Month, 1).AddMonths(1);

    public static bool IsInUpcomingMonth(DateOnly date, DateOnly today)
    {
        var upcomingMonth = GetUpcomingMonth(today);

        return date.Year == upcomingMonth.Year && date.Month == upcomingMonth.Month;
    }

    public static string GetMonthlyPeriodKey(DateOnly today) => GetUpcomingMonth(today).ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public static bool IsInChristmasSeason(DateOnly today) =>
        (today.Month == 11) || (today.Month == 12 && today.Day <= 24);

    public static string GetChristmasPeriodKey(DateOnly today)
    {
        var date = today.ToDateTime(TimeOnly.MinValue);

        return $"{ISOWeek.GetYear(date)}-W{ISOWeek.GetWeekOfYear(date):D2}";
    }
}
