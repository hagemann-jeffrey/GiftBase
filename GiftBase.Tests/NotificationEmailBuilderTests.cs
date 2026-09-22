using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Features.Notifications;
using Shouldly;

namespace GiftBase.Tests;

public class NotificationEmailBuilderTests
{
    private static readonly DateOnly Today = new(2026, 11, 3);
    private const string BaseUrl = "https://giftbase.example.com";

    private static NotificationEntry MonthlyEntry(
        Person person, Occasion occasion, int personId = 42, IReadOnlyList<string>? giftIdeas = null) =>
        new()
        {
            PersonId = personId,
            Person = person,
            Occasion = occasion,
            GiftIdeas = giftIdeas ?? []
        };

    private static NotificationEntry ChristmasEntry(
        Person person, int personId, IReadOnlyList<string>? giftIdeas = null) =>
        new()
        {
            PersonId = personId,
            Person = person,
            Occasion = new Occasion(OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true, personId),
            GiftIdeas = giftIdeas ?? []
        };

    [Fact]
    public void BuildMonthlyOverview_ShouldShowTheUpcomingMonthInSubject()
    {
        // Arrange: today is 3. November -> upcoming month is Dezember
        var person = new Person("Anna", "Muster", new DateOnly(1992, 12, 20), Relation.Friend, 1);
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(1992, 12, 20), true, 42);

        // Act
        var email = NotificationEmailBuilder.BuildMonthlyOverview([MonthlyEntry(person, occasion)], Today, BaseUrl);

        // Assert
        email.Subject.ShouldContain("Dezember 2026");
    }

    [Fact]
    public void BuildMonthlyOverview_ShouldIncludePersonNameOccasionAndDateInBody()
    {
        // Arrange
        var person = new Person("Anna", "Muster", null, Relation.Friend, 1);
        var occasion = new Occasion(OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false, 42);

        // Act
        var email = NotificationEmailBuilder.BuildMonthlyOverview([MonthlyEntry(person, occasion)], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain("Anna Muster");
        email.Body.ShouldContain("Konfirmation");
        email.Body.ShouldContain("20. Dezember 2026");
    }

    [Fact]
    public void BuildMonthlyOverview_ShouldIncludeAge_ForBirthdayOccasions()
    {
        // Arrange
        var person = new Person("Anna", "Muster", new DateOnly(1992, 12, 20), Relation.Friend, 1);
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(1992, 12, 20), true, 42);

        // Act
        var email = NotificationEmailBuilder.BuildMonthlyOverview([MonthlyEntry(person, occasion)], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain("34");
    }

    [Fact]
    public void BuildMonthlyOverview_ShouldLinkToPersonDetailPage()
    {
        // Arrange
        var person = new Person("Anna", "Muster", null, Relation.Friend, 1);
        var occasion = new Occasion(OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false, 42);

        // Act
        var email = NotificationEmailBuilder.BuildMonthlyOverview([MonthlyEntry(person, occasion)], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain($"{BaseUrl}/persons/42");
    }

    [Fact]
    public void BuildMonthlyOverview_ShouldListGiftIdeasLinkedToTheOccasion()
    {
        // Arrange
        var person = new Person("Anna", "Muster", null, Relation.Friend, 1);
        var occasion = new Occasion(OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false, 42);

        // Act
        var email = NotificationEmailBuilder.BuildMonthlyOverview(
            [MonthlyEntry(person, occasion, giftIdeas: ["Kopfhörer", "Kochbuch"])], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain("Kopfhörer");
        email.Body.ShouldContain("Kochbuch");
    }

    [Fact]
    public void BuildMonthlyOverview_ShouldHighlightMissingOccasionGiftIdeas()
    {
        // Arrange
        var person = new Person("Anna", "Muster", null, Relation.Friend, 1);
        var occasion = new Occasion(OccasionType.Custom, "Konfirmation", new DateOnly(2026, 12, 20), false, 42);

        // Act
        var email = NotificationEmailBuilder.BuildMonthlyOverview([MonthlyEntry(person, occasion)], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain("Noch keine Geschenkidee für diesen Anlass");
    }

    [Fact]
    public void BuildChristmasReminder_ShouldIncludeCountdownInSubject()
    {
        // Arrange
        var person = new Person("Anna", "Muster", null, Relation.Friend, 1);

        // Act
        var email = NotificationEmailBuilder.BuildChristmasReminder([ChristmasEntry(person, 42)], Today, BaseUrl);

        // Assert
        email.Subject.ShouldContain("51 Tagen");
    }

    [Fact]
    public void BuildChristmasReminder_ShouldListGiftIdeasLinkedToChristmas()
    {
        // Arrange
        var person = new Person("Ben", "Beispiel", null, Relation.Friend, 1);

        // Act
        var email = NotificationEmailBuilder.BuildChristmasReminder(
            [ChristmasEntry(person, 2, giftIdeas: ["Kopfhörer", "Kochbuch"])], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain("Kopfhörer");
        email.Body.ShouldContain("Kochbuch");
    }

    [Fact]
    public void BuildChristmasReminder_ShouldHighlightPersonsWithoutOccasionGiftIdeas()
    {
        // Arrange
        var withoutGift = new Person("Anna", "Muster", null, Relation.Friend, 1);
        var withGift = new Person("Ben", "Beispiel", null, Relation.Friend, 1);

        // Act
        var email = NotificationEmailBuilder.BuildChristmasReminder(
            [ChristmasEntry(withoutGift, 1), ChristmasEntry(withGift, 2, giftIdeas: ["Kochbuch"])], Today, BaseUrl);

        // Assert
        email.Body.ShouldContain("Noch keine Geschenkidee für diesen Anlass");
    }
}
