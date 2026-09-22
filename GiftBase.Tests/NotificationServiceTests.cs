using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Interfaces;
using GiftBase.Features.Notifications;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace GiftBase.Tests;

public class NotificationServiceTests
{
    private const string BaseUrl = "https://giftbase.example.com";

    private readonly TestDbContextFactory _dbContextFactory;
    private readonly IEmailService _emailService;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _emailService = Substitute.For<IEmailService>();
        _notificationService = new NotificationService(_dbContextFactory, _emailService);
    }

    private async Task<User> AddUserAsync(bool isEmailVerified = true)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = new User($"{Guid.NewGuid():N}@iu-study.org");
        user.SetPasswordHash("hash");
        if (isEmailVerified)
        {
            user.ConfirmEmail();
        }
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    private async Task<Person> AddPersonAsync(int userId, bool notificationsEnabled = true, string firstName = "Anna")
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person(firstName, "Muster", null, Relation.Friend, userId, notificationsEnabled: notificationsEnabled);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        return person;
    }

    private async Task<Occasion> AddOccasionAsync(int personId, OccasionType type, string? title, DateOnly date, bool isRecurring)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var occasion = new Occasion(type, title, date, isRecurring, personId);
        dbContext.Occasions.Add(occasion);
        await dbContext.SaveChangesAsync();

        return occasion;
    }

    private async Task AddGiftAsync(int personId, string title, GiftStatus status = GiftStatus.Idea, int? occasionId = null)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var gift = new Gift(title, null, null, null, personId, occasionId);

        if (status != GiftStatus.Idea)
        {
            gift.Update(new Core.Dtos.GiftUpdateDto { Title = title, Status = status, OccasionId = occasionId });
        }

        dbContext.Gifts.Add(gift);
        await dbContext.SaveChangesAsync();
    }

    private async Task<int> CountNotificationLogsAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.NotificationLogs.CountAsync();
    }

    // Subject/Body sind nur im Dry-Run befüllt - der Aufrufer belegt damit zugleich, dass er einen erwartet.
    private static string DryRunBody(Core.Dtos.NotificationDispatchResultDto result) =>
        result.Dispatches[0].Body.ShouldNotBeNull();

    [Fact]
    public async Task DispatchAsync_ShouldSendMonthlyOverview_WhenOccasionIsInUpcomingMonth()
    {
        // Arrange: heute 3. November -> kommender Monat ist Dezember
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);

        // Assert
        result.Dispatches.Count.ShouldBe(1);
        result.Dispatches[0].Kind.ShouldBe(NotificationKind.MonthlyOccasions);
        result.Dispatches[0].PeriodKey.ShouldBe("2026-12");
        await _emailService.Received(1).SendEmailAsync(user.Email, Arg.Any<string>(), Arg.Any<string>());
        (await CountNotificationLogsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task DispatchAsync_ShouldNotIncludeOccasion_WhenItIsInTheCurrentMonth()
    {
        // Arrange: heute 3. November, Anlass noch im November -> darf nicht in der Dezember-Vorschau landen
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 11, 20), false);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);

        // Assert
        result.Dispatches.ShouldBeEmpty();
    }

    [Fact]
    public async Task DispatchAsync_ShouldNotSendTwice_ForSamePeriod()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);

        // Act
        await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);
        var secondResult = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);

        // Assert
        secondResult.Dispatches.ShouldBeEmpty();
        await _emailService.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        (await CountNotificationLogsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task DispatchAsync_ShouldExcludeMutedPersons_FromMonthlyOverview()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var mutedPerson = await AddPersonAsync(user.Id, notificationsEnabled: false, firstName: "Muted");
        await AddOccasionAsync(mutedPerson.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: true);

        // Assert
        result.Dispatches.ShouldBeEmpty();
    }

    [Fact]
    public async Task DispatchAsync_ShouldNotSend_ForUnverifiedUser()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync(isEmailVerified: false);
        var person = await AddPersonAsync(user.Id);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);

        // Assert
        result.Dispatches.ShouldBeEmpty();
        await _emailService.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task DispatchAsync_ShouldNotSend_WhenUserHasNoQualifyingOccasions()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        await AddPersonAsync(user.Id);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);

        // Assert
        result.Dispatches.ShouldBeEmpty();
        (await CountNotificationLogsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task DispatchAsync_DryRun_ShouldNotSendEmailOrWriteLog()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: true);

        // Assert
        result.Dispatches.Count.ShouldBe(1);
        DryRunBody(result).ShouldContain("Anna Muster");
        await _emailService.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        (await CountNotificationLogsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task DispatchAsync_ShouldIncludeGiftIdeasLinkedToOccasion_InMonthlyOverview()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);
        await AddGiftAsync(person.Id, "Kopfhörer", GiftStatus.Idea, occasion.Id);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: true);

        // Assert
        DryRunBody(result).ShouldContain("Kopfhörer");
    }

    [Fact]
    public async Task DispatchAsync_ShouldExcludeNonIdeaGifts_FromMonthlyOverview()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);
        await AddGiftAsync(person.Id, "Bereits gekauft", GiftStatus.Bought, occasion.Id);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: true);

        // Assert
        DryRunBody(result).ShouldNotContain("Bereits gekauft");
    }

    [Fact]
    public async Task DispatchAsync_ShouldContinueAndNotLog_WhenEmailSendThrows()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 3);
        var failingUser = await AddUserAsync();
        var failingPerson = await AddPersonAsync(failingUser.Id, firstName: "Failing");
        await AddOccasionAsync(failingPerson.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false);

        var succeedingUser = await AddUserAsync();
        var succeedingPerson = await AddPersonAsync(succeedingUser.Id, firstName: "Succeeding");
        await AddOccasionAsync(succeedingPerson.Id, OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 21), false);

        _emailService.SendEmailAsync(failingUser.Email, Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => Task.FromException(new InvalidOperationException("SMTP nicht erreichbar")));

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: false);

        // Assert
        result.Dispatches.Count.ShouldBe(2);
        var failingDispatch = result.Dispatches.Single(d => d.Recipient == failingUser.Email);
        failingDispatch.Error.ShouldNotBeNull();
        var succeedingDispatch = result.Dispatches.Single(d => d.Recipient == succeedingUser.Email);
        succeedingDispatch.Error.ShouldBeNull();
        (await CountNotificationLogsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task DispatchAsync_ShouldSendChristmasReminder_DuringChristmasSeason()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 16);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        await AddOccasionAsync(person.Id, OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: true);

        // Assert
        result.Dispatches.Count.ShouldBe(1);
        result.Dispatches[0].Kind.ShouldBe(NotificationKind.ChristmasReminder);
    }

    [Fact]
    public async Task DispatchAsync_ShouldIncludeGiftIdeasLinkedToChristmasOccasion()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 16);
        var user = await AddUserAsync();
        var person = await AddPersonAsync(user.Id);
        var occasion = await AddOccasionAsync(person.Id, OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true);
        await AddGiftAsync(person.Id, "Kochbuch", GiftStatus.Idea, occasion.Id);

        // Act
        var result = await _notificationService.DispatchAsync(today, BaseUrl, dryRun: true);

        // Assert
        DryRunBody(result).ShouldContain("Kochbuch");
    }

}
