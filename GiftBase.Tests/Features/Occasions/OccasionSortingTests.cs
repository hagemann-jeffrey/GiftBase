using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Features.Occasions;
using Shouldly;

namespace GiftBase.Tests.Features.Occasions;

public class OccasionSortingTests
{
    private static readonly DateOnly Today = new(2026, 5, 12);

    [Fact]
    public void SortByNextOccurrence_ShouldOrderByNextDate()
    {
        // Arrange
        var christmas = new Occasion(OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true, 1);
        var birthday = new Occasion(OccasionType.Birthday, null, new DateOnly(1990, 5, 24), true, 1);
        var wedding = new Occasion(OccasionType.Custom, "Hochzeit", new DateOnly(2026, 6, 20), false, 1);

        // Act
        var sorted = new[] { christmas, wedding, birthday }.SortByNextOccurrence(Today);

        // Assert
        sorted[0].ShouldBe(birthday);
        sorted[1].ShouldBe(wedding);
        sorted[2].ShouldBe(christmas);
    }

    [Fact]
    public void SortByNextOccurrence_ShouldPutPastOneTimeOccasionsLast()
    {
        // Arrange
        var past = new Occasion(OccasionType.Custom, "Studienabschluss", new DateOnly(2025, 9, 30), false, 1);
        var upcoming = new Occasion(OccasionType.Christmas, null, new DateOnly(2026, 12, 24), true, 1);

        // Act
        var sorted = new[] { past, upcoming }.SortByNextOccurrence(Today);

        // Assert
        sorted[0].ShouldBe(upcoming);
        sorted[1].ShouldBe(past);
    }

    [Fact]
    public void SortByNextOccurrence_ShouldReturnEmptyList_WhenThereAreNoOccasions()
    {
        // Act
        var sorted = Array.Empty<Occasion>().SortByNextOccurrence(Today);

        // Assert
        sorted.ShouldBeEmpty();
    }
}
