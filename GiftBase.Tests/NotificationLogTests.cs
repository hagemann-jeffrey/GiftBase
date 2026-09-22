using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using Shouldly;

namespace GiftBase.Tests;

public class NotificationLogTests
{
    [Fact]
    public void NotificationLog_ShouldBeCreated()
    {
        // Arrange
        var sentAtUtc = new DateTime(2026, 11, 3, 6, 0, 0, DateTimeKind.Utc);

        // Act
        var notificationLog = new NotificationLog(7, NotificationKind.MonthlyOccasions, "2026-11", sentAtUtc);

        // Assert
        notificationLog.UserId.ShouldBe(7);
        notificationLog.Kind.ShouldBe(NotificationKind.MonthlyOccasions);
        notificationLog.PeriodKey.ShouldBe("2026-11");
        notificationLog.SentAtUtc.ShouldBe(sentAtUtc);
    }
}
