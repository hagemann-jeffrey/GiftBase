using GiftBase.Core.Dtos.Occasions;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using Shouldly;

namespace GiftBase.Tests.Features.Occasions;

public class OccasionTests
{
    [Fact]
    public void Occasion_ShouldBeCreated()
    {
        // Act
        var occasion = new Occasion(OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), true, 1);

        // Assert
        occasion.Type.ShouldBe(OccasionType.Custom);
        occasion.Title.ShouldBe("Hochzeit");
        occasion.Date.ShouldBe(new DateOnly(2027, 6, 5));
        occasion.IsRecurring.ShouldBeTrue();
        occasion.PersonId.ShouldBe(1);
    }

    [Fact]
    public void Occasion_ShouldBeCreated_WithoutTitle()
    {
        // Act
        var occasion = new Occasion(OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true, 1);

        // Assert
        occasion.Title.ShouldBeNull();
    }

    [Fact]
    public void Occasion_ShouldBeUpdated()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), true, 1);

        // Act
        occasion.Update(new OccasionUpdateDto
        {
            Title = "Studienabschluss",
            Date = new DateOnly(2027, 9, 30),
            IsRecurring = false
        });

        // Assert
        occasion.Title.ShouldBe("Studienabschluss");
        occasion.Date.ShouldBe(new DateOnly(2027, 9, 30));
        occasion.IsRecurring.ShouldBeFalse();
    }

    [Fact]
    public void Occasion_ShouldNotChangeTypeAndPersonId_WhenUpdated()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Hochzeit", new DateOnly(2027, 6, 5), true, 7);

        // Act
        occasion.Update(new OccasionUpdateDto
        {
            Title = "Studienabschluss",
            Date = new DateOnly(2027, 9, 30),
            IsRecurring = false
        });

        // Assert
        occasion.Type.ShouldBe(OccasionType.Custom);
        occasion.PersonId.ShouldBe(7);
    }

    [Fact]
    public void SetDate_ShouldChangeDate()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(1990, 3, 14), true, 1);

        // Act
        occasion.SetDate(new DateOnly(1991, 4, 15));

        // Assert
        occasion.Date.ShouldBe(new DateOnly(1991, 4, 15));
    }

    [Fact]
    public void GetNextOccurrence_ShouldReturnDate_WhenOccasionIsNotRecurring()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Studienabschluss", new DateOnly(2027, 9, 30), false, 1);

        // Act
        var nextOccurrence = occasion.GetNextOccurrence(new DateOnly(2026, 5, 12));

        // Assert
        nextOccurrence.ShouldBe(new DateOnly(2027, 9, 30));
    }

    [Fact]
    public void GetNextOccurrence_ShouldReturnDateInCurrentYear_WhenAnniversaryIsStillAhead()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(1990, 5, 24), true, 1);

        // Act
        var nextOccurrence = occasion.GetNextOccurrence(new DateOnly(2026, 5, 12));

        // Assert
        nextOccurrence.ShouldBe(new DateOnly(2026, 5, 24));
    }

    [Fact]
    public void GetNextOccurrence_ShouldReturnToday_WhenAnniversaryIsToday()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(1990, 5, 24), true, 1);

        // Act
        var nextOccurrence = occasion.GetNextOccurrence(new DateOnly(2026, 5, 24));

        // Assert
        nextOccurrence.ShouldBe(new DateOnly(2026, 5, 24));
    }

    [Fact]
    public void GetNextOccurrence_ShouldReturnDateInNextYear_WhenAnniversaryHasPassed()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true, 1);

        // Act
        var nextOccurrence = occasion.GetNextOccurrence(new DateOnly(2026, 12, 25));

        // Assert
        nextOccurrence.ShouldBe(new DateOnly(2027, 12, 24));
    }

    [Fact]
    public void GetNextOccurrence_ShouldClampToLastDayOfMonth_WhenLeapDayIsMissingInTargetYear()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(2000, 2, 29), true, 1);

        // Act
        var nextOccurrence = occasion.GetNextOccurrence(new DateOnly(2027, 1, 1));

        // Assert
        nextOccurrence.ShouldBe(new DateOnly(2027, 2, 28));
    }

    [Fact]
    public void IsPast_ShouldBeTrue_WhenOneTimeOccasionIsBeforeToday()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Studienabschluss", new DateOnly(2025, 9, 30), false, 1);

        // Act
        var isPast = occasion.IsPast(new DateOnly(2026, 5, 12));

        // Assert
        isPast.ShouldBeTrue();
    }

    [Fact]
    public void IsPast_ShouldBeFalse_WhenOccasionIsRecurring()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Birthday, null, new DateOnly(1990, 1, 1), true, 1);

        // Act
        var isPast = occasion.IsPast(new DateOnly(2026, 5, 12));

        // Assert
        isPast.ShouldBeFalse();
    }
}
