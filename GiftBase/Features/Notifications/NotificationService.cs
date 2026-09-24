using GiftBase.Core.Dtos.Notifications;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.Notifications;

public class NotificationService(IDbContextFactory<GiftBaseDbContext> dbContextFactory, IEmailService emailService) : INotificationService
{
    public async Task<NotificationDispatchResultDto> DispatchAsync(DateOnly today, string baseUrl, bool dryRun)
    {
        var result = new NotificationDispatchResultDto { Today = today, DryRun = dryRun };

        var isMonthlyWindow = NotificationSchedule.IsMonthlyWindow(today);
        var isChristmasSeason = NotificationSchedule.IsInChristmasSeason(today);

        if (!isMonthlyWindow && !isChristmasSeason)
        {
            return result;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var users = await dbContext.Users
            .Where(u => u.IsEmailVerified)
            .Include(u => u.Persons.Where(p => p.NotificationsEnabled))
                .ThenInclude(p => p.Occasions)
            .Include(u => u.Persons.Where(p => p.NotificationsEnabled))
                .ThenInclude(p => p.Gifts)
            .ToListAsync();

        var monthlyPeriodKey = NotificationSchedule.GetMonthlyPeriodKey(today);
        var christmasPeriodKey = NotificationSchedule.GetChristmasPeriodKey(today);

        var userIds = users.Select(u => u.Id).ToList();
        var existingLogs = await dbContext.NotificationLogs
            .Where(n => userIds.Contains(n.UserId))
            .Where(n =>
                (n.Kind == NotificationKind.MonthlyOccasions && n.PeriodKey == monthlyPeriodKey) ||
                (n.Kind == NotificationKind.ChristmasReminder && n.PeriodKey == christmasPeriodKey))
            .ToListAsync();

        foreach (var user in users)
        {
            if (isMonthlyWindow && !HasLog(existingLogs, user.Id, NotificationKind.MonthlyOccasions, monthlyPeriodKey))
            {
                await DispatchMonthlyOverviewAsync(dbContext, user, today, baseUrl, dryRun, monthlyPeriodKey, result.Dispatches);
            }

            if (isChristmasSeason && !HasLog(existingLogs, user.Id, NotificationKind.ChristmasReminder, christmasPeriodKey))
            {
                await DispatchChristmasReminderAsync(dbContext, user, today, baseUrl, dryRun, christmasPeriodKey, result.Dispatches);
            }
        }

        if (!dryRun)
        {
            await dbContext.SaveChangesAsync();
        }

        return result;
    }

    private async Task DispatchMonthlyOverviewAsync(
        GiftBaseDbContext dbContext, User user, DateOnly today, string baseUrl, bool dryRun, string periodKey, List<NotificationDispatchDto> dispatches)
    {
        var entries = BuildEntries(user, o =>
            (o.Type == OccasionType.Birthday || o.Type == OccasionType.Custom)
            && !o.IsPast(today)
            && NotificationSchedule.IsInUpcomingMonth(o.GetNextOccurrence(today), today));

        if (entries.Count == 0)
        {
            return;
        }

        var (subject, body) = NotificationEmailBuilder.BuildMonthlyOverview(entries, today, baseUrl);

        dispatches.Add(await SendAsync(dbContext, user, NotificationKind.MonthlyOccasions, periodKey, subject, body, dryRun));
    }

    private async Task DispatchChristmasReminderAsync(
        GiftBaseDbContext dbContext, User user, DateOnly today, string baseUrl, bool dryRun, string periodKey, List<NotificationDispatchDto> dispatches)
    {
        var entries = BuildEntries(user, o => o.Type == OccasionType.Christmas);

        if (entries.Count == 0)
        {
            return;
        }

        var (subject, body) = NotificationEmailBuilder.BuildChristmasReminder(entries, today, baseUrl);

        dispatches.Add(await SendAsync(dbContext, user, NotificationKind.ChristmasReminder, periodKey, subject, body, dryRun));
    }

    private static List<NotificationEntry> BuildEntries(User user, Func<Occasion, bool> isRelevant) =>
        [.. user.Persons.SelectMany(p => p.Occasions
            .Where(isRelevant)
            .Select(o => new NotificationEntry
            {
                PersonId = p.Id,
                Person = p,
                Occasion = o,
                GiftIdeas = [.. p.Gifts
                    .Where(g => g.OccasionId == o.Id && g.Status == GiftStatus.Idea)
                    .Select(g => g.Title)]
            }))];

    private async Task<NotificationDispatchDto> SendAsync(
        GiftBaseDbContext dbContext, User user, NotificationKind kind, string periodKey, string subject, string body, bool dryRun)
    {
        var dispatch = new NotificationDispatchDto
        {
            Recipient = user.Email,
            Kind = kind,
            PeriodKey = periodKey,
            Subject = dryRun ? subject : null,
            Body = dryRun ? body : null
        };

        if (dryRun)
        {
            return dispatch;
        }

        try
        {
            await emailService.SendEmailAsync(user.Email, subject, body);
            dbContext.NotificationLogs.Add(new NotificationLog(user.Id, kind, periodKey, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            dispatch.Error = ex.Message;
        }

        return dispatch;
    }

    private static bool HasLog(List<NotificationLog> logs, int userId, NotificationKind kind, string periodKey) =>
        logs.Any(l => l.UserId == userId && l.Kind == kind && l.PeriodKey == periodKey);
}
