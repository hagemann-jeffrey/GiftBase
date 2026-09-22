using GiftBase.Features.Notifications;
using Shouldly;

namespace GiftBase.Tests;

public class NotificationScheduleTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(7, true)]
    [InlineData(8, false)]
    public void IsMonthlyWindow_ShouldOnlyBeTrueWithinFirstSevenDays(int day, bool expected)
    {
        // Arrange
        var today = new DateOnly(2026, 11, day);

        // Act & Assert
        NotificationSchedule.IsMonthlyWindow(today).ShouldBe(expected);
    }

    [Fact]
    public void GetMonthlyPeriodKey_ShouldFormatAsYearDashMonth_ForTheUpcomingMonth()
    {
        // Arrange
        var today = new DateOnly(2026, 3, 5);

        // Act & Assert
        NotificationSchedule.GetMonthlyPeriodKey(today).ShouldBe("2026-04");
    }

    [Fact]
    public void GetMonthlyPeriodKey_ShouldRollOverToNextYear_WhenSentInDecember()
    {
        // Arrange
        var today = new DateOnly(2026, 12, 3);

        // Act & Assert
        NotificationSchedule.GetMonthlyPeriodKey(today).ShouldBe("2027-01");
    }

    [Fact]
    public void GetUpcomingMonth_ShouldReturnFirstDayOfNextMonth()
    {
        // Arrange
        var today = new DateOnly(2026, 3, 5);

        // Act & Assert
        NotificationSchedule.GetUpcomingMonth(today).ShouldBe(new DateOnly(2026, 4, 1));
    }

    [Fact]
    public void GetUpcomingMonth_ShouldRollOverToJanuary_WhenTodayIsInDecember()
    {
        // Arrange
        var today = new DateOnly(2026, 12, 20);

        // Act & Assert
        NotificationSchedule.GetUpcomingMonth(today).ShouldBe(new DateOnly(2027, 1, 1));
    }

    [Theory]
    [InlineData(2026, 3, 31, false)]
    [InlineData(2026, 4, 1, true)]
    [InlineData(2026, 4, 30, true)]
    [InlineData(2026, 5, 1, false)]
    public void IsInUpcomingMonth_ShouldOnlyBeTrueForNextCalendarMonth(int year, int month, int day, bool expected)
    {
        // Arrange
        var today = new DateOnly(2026, 3, 5);
        var date = new DateOnly(year, month, day);

        // Act & Assert
        NotificationSchedule.IsInUpcomingMonth(date, today).ShouldBe(expected);
    }

    [Fact]
    public void IsInUpcomingMonth_ShouldHandleYearBoundary()
    {
        // Arrange
        var today = new DateOnly(2026, 12, 3);

        // Act & Assert
        NotificationSchedule.IsInUpcomingMonth(new DateOnly(2027, 1, 15), today).ShouldBeTrue();
        NotificationSchedule.IsInUpcomingMonth(new DateOnly(2026, 12, 15), today).ShouldBeFalse();
    }

    [Theory]
    [InlineData(2026, 10, 31, false)]
    [InlineData(2026, 11, 1, true)]
    [InlineData(2026, 11, 30, true)]
    [InlineData(2026, 12, 1, true)]
    [InlineData(2026, 12, 24, true)]
    [InlineData(2026, 12, 25, false)]
    public void IsInChristmasSeason_ShouldOnlyBeTrueFromNovemberFirstToDecemberTwentyFourth(int year, int month, int day, bool expected)
    {
        // Arrange
        var today = new DateOnly(year, month, day);

        // Act & Assert
        NotificationSchedule.IsInChristmasSeason(today).ShouldBe(expected);
    }

    [Fact]
    public void GetChristmasPeriodKey_ShouldFormatAsIsoYearDashWeek()
    {
        // Arrange
        var today = new DateOnly(2026, 11, 2);

        // Act & Assert
        NotificationSchedule.GetChristmasPeriodKey(today).ShouldBe("2026-W45");
    }

    [Fact]
    public void GetChristmasPeriodKey_ShouldBeSameKey_ForEntireIsoWeek()
    {
        // Arrange
        var monday = new DateOnly(2026, 12, 21);
        var thursday = new DateOnly(2026, 12, 24);

        // Act & Assert
        NotificationSchedule.GetChristmasPeriodKey(thursday).ShouldBe(NotificationSchedule.GetChristmasPeriodKey(monday));
    }
}
