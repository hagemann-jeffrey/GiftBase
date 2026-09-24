using System.Globalization;
using System.Text;
using GiftBase.Core.Enums;
using GiftBase.Features.Occasions;
using GiftBase.Shared.Common;

namespace GiftBase.Features.Notifications;

public static class NotificationEmailBuilder
{
    public static (string Subject, string Body) BuildMonthlyOverview(
        IReadOnlyList<NotificationEntry> entries,
        DateOnly today,
        string baseUrl)
    {
        var monthName = NotificationSchedule.GetUpcomingMonth(today).ToString("MMMM yyyy", AppCulture.German);

        return Build(
            subject: $"Deine Anlässe im {monthName}",
            heading: $"Anlässe im {monthName}",
            intro: "Diese Anlässe stehen im kommenden Monat an:",
            entries: entries.OrderBy(e => e.Occasion.GetNextOccurrence(today)),
            today, baseUrl);
    }

    public static (string Subject, string Body) BuildChristmasReminder(
        IReadOnlyList<NotificationEntry> entries,
        DateOnly today,
        string baseUrl)
    {
        var daysUntilChristmas = new DateOnly(today.Year, 12, 24).DayNumber - today.DayNumber;

        return Build(
            subject: $"Weihnachten rückt näher – noch {daysUntilChristmas} Tagen",
            heading: "Weihnachten steht vor der Tür",
            intro: $"Noch {daysUntilChristmas} Tage bis Heiligabend. So ist der Stand bei deinen Geschenkideen:",
            entries: entries.OrderBy(e => e.GiftIdeas.Count),
            today, baseUrl);
    }

    private static (string Subject, string Body) Build(
        string subject,
        string heading,
        string intro,
        IEnumerable<NotificationEntry> entries,
        DateOnly today,
        string baseUrl)
    {
        var itemsHtml = new StringBuilder();

        foreach (var entry in entries)
        {
            var person = entry.Person;

            itemsHtml.Append(CultureInfo.InvariantCulture, $@"
                <li style='margin-bottom: 14px;'>
                    <a href='{baseUrl}/persons/{entry.PersonId}' style='color: #594AE2; text-decoration: none; font-weight: bold;'>{person.FirstName} {person.LastName}</a>{BuildOccasionHtml(entry, today)}
                    {BuildGiftIdeasHtml(entry.GiftIdeas)}
                </li>");
        }

        var body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>{heading}</h2>
                <p>{intro}</p>
                <ul style='list-style: none; padding: 0;'>{itemsHtml}</ul>
            </div>";

        return (subject, body);
    }

    private static string BuildOccasionHtml(NotificationEntry entry, DateOnly today)
    {
        var occasion = entry.Occasion;

        if (occasion.Type == OccasionType.Christmas)
        {
            return string.Empty;
        }

        var age = entry.Person.GetAge(occasion.GetNextOccurrence(today));
        var ageSuffix = occasion.Type == OccasionType.Birthday && age.HasValue ? $" (wird {age})" : string.Empty;

        return $@"
                    – {Translations.GetOccasionDisplayTitle(occasion)}{ageSuffix}<br/>
                    <span style='color: #666;'>{occasion.FormatNextOccurrence(today)} ({occasion.FormatCountdown(today)})</span>";
    }

    private static string BuildGiftIdeasHtml(IReadOnlyList<string> giftIdeas) =>
        giftIdeas.Count > 0
            ? $"<div style='margin-top: 4px;'>Ideen für diesen Anlass: {string.Join(", ", giftIdeas)}</div>"
            : "<div style='margin-top: 4px; color: #c62828; font-weight: bold;'>Noch keine Geschenkidee für diesen Anlass</div>";
}
