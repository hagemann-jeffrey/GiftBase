using GiftBase.Core.Entities;
using Shouldly;

namespace GiftBase.Tests;

public class ShareLinkTests
{
    private static readonly DateTime CreatedAt = new(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_ShouldSetAllValues_WhenShareLinkIsScopedToThePerson()
    {
        // Act
        var shareLink = new ShareLink("abc123", 7, 1, null, CreatedAt);

        // Assert
        shareLink.Token.ShouldBe("abc123");
        shareLink.UserId.ShouldBe(7);
        shareLink.PersonId.ShouldBe(1);
        shareLink.OccasionId.ShouldBeNull();
        shareLink.CreatedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldKeepOccasionId_WhenShareLinkIsScopedToAnOccasion()
    {
        // Act
        var shareLink = new ShareLink("abc123", 7, 1, 42, CreatedAt);

        // Assert
        shareLink.OccasionId.ShouldBe(42);
    }

    [Fact]
    public void Lifetime_ShouldBeThirtyDays()
    {
        // Assert
        ShareLink.Lifetime.ShouldBe(TimeSpan.FromDays(30));
    }

    [Fact]
    public void ExpiresAt_ShouldBeThirtyDaysAfterCreation()
    {
        // Arrange
        var shareLink = new ShareLink("abc123", 7, 1, null, CreatedAt);

        // Assert
        shareLink.ExpiresAt.ShouldBe(new DateTime(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenLinkIsOneDayOld()
    {
        // Arrange
        var shareLink = new ShareLink("abc123", 7, 1, null, CreatedAt);

        // Act & Assert
        shareLink.IsExpired(CreatedAt.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenLinkIsOneSecondBeforeExpiry()
    {
        // Arrange
        var shareLink = new ShareLink("abc123", 7, 1, null, CreatedAt);

        // Act & Assert
        shareLink.IsExpired(CreatedAt.AddDays(30).AddSeconds(-1)).ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenLinkIsExactlyThirtyDaysOld()
    {
        // Arrange
        var shareLink = new ShareLink("abc123", 7, 1, null, CreatedAt);

        // Act & Assert
        shareLink.IsExpired(CreatedAt.AddDays(30)).ShouldBeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenLinkIsOlderThanThirtyDays()
    {
        // Arrange
        var shareLink = new ShareLink("abc123", 7, 1, null, CreatedAt);

        // Act & Assert
        shareLink.IsExpired(CreatedAt.AddDays(31)).ShouldBeTrue();
    }
}
