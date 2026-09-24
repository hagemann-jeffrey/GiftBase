using GiftBase.Core.Entities;
using Shouldly;

namespace GiftBase.Tests.Features.GiftSuggestions;

public class GiftSuggestionQuotaTests
{
    private static readonly DateTime Now = new(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Window_ShouldBeOneDay()
    {
        // Assert
        GiftSuggestionQuota.Window.ShouldBe(TimeSpan.FromDays(1));
    }

    [Fact]
    public void MaxRequestsPerWindow_ShouldBeFive()
    {
        // Assert
        GiftSuggestionQuota.MaxRequestsPerWindow.ShouldBe(5);
    }

    [Fact]
    public void Constructor_ShouldSetUserId()
    {
        // Act
        var quota = new GiftSuggestionQuota(7);

        // Assert
        quota.UserId.ShouldBe(7);
        quota.RequestCount.ShouldBe(0);
    }

    [Fact]
    public void RegisterRequest_ShouldSetCountToOne_WhenFirstRequest()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);

        // Act
        quota.RegisterRequest(Now);

        // Assert
        quota.RequestCount.ShouldBe(1);
        quota.WindowStartedAt.ShouldBe(Now);
    }

    [Fact]
    public void RegisterRequest_ShouldIncrementCount_WhenWithinWindow()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);
        quota.RegisterRequest(Now);

        // Act
        quota.RegisterRequest(Now.AddMinutes(10));

        // Assert
        quota.RequestCount.ShouldBe(2);
        quota.WindowStartedAt.ShouldBe(Now);
    }

    [Fact]
    public void RegisterRequest_ShouldResetCountToOne_WhenWindowHasElapsed()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);
        quota.RegisterRequest(Now);

        // Act
        var afterWindow = Now.AddDays(1);
        quota.RegisterRequest(afterWindow);

        // Assert
        quota.RequestCount.ShouldBe(1);
        quota.WindowStartedAt.ShouldBe(afterWindow);
    }

    [Fact]
    public void IsExhausted_ShouldReturnFalse_WhenNoRequestsRegisteredYet()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);

        // Act & Assert
        quota.IsExhausted(Now).ShouldBeFalse();
    }

    [Fact]
    public void IsExhausted_ShouldReturnFalse_WhenBelowMaxWithinWindow()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);
        for (var i = 0; i < 4; i++)
        {
            quota.RegisterRequest(Now.AddMinutes(i));
        }

        // Act & Assert
        quota.IsExhausted(Now.AddMinutes(5)).ShouldBeFalse();
    }

    [Fact]
    public void IsExhausted_ShouldReturnTrue_WhenMaxReachedWithinWindow()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);
        for (var i = 0; i < 5; i++)
        {
            quota.RegisterRequest(Now.AddMinutes(i));
        }

        // Act & Assert
        quota.IsExhausted(Now.AddMinutes(6)).ShouldBeTrue();
    }

    [Fact]
    public void IsExhausted_ShouldReturnFalse_WhenWindowHasElapsedSinceMaxWasReached()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);
        for (var i = 0; i < 5; i++)
        {
            quota.RegisterRequest(Now.AddMinutes(i));
        }

        // Act & Assert
        quota.IsExhausted(Now.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void ResetsAt_ShouldBeWindowStartPlusOneDay()
    {
        // Arrange
        var quota = new GiftSuggestionQuota(7);
        quota.RegisterRequest(Now);

        // Act & Assert
        quota.ResetsAt.ShouldBe(Now.AddDays(1));
    }
}
