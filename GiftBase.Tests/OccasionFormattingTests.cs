using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Features.Occasions;
using MudBlazor;
using Shouldly;

namespace GiftBase.Tests;

public class OccasionFormattingTests
{
    private static readonly DateOnly Today = new(2026, 5, 12);

    [Theory]
    [InlineData(OccasionType.Birthday, Icons.Material.Filled.Cake)]
    [InlineData(OccasionType.Christmas, Icons.Material.Filled.Park)]
    [InlineData(OccasionType.Custom, Icons.Material.Filled.Event)]
    public void GetIcon_ShouldReturnIconForType(OccasionType type, string expectedIcon)
    {
        // Arrange
        var occasion = new Occasion(type, "Jubiläum", Today, false, 1);

        // Act
        var icon = occasion.GetIcon();

        // Assert
        icon.ShouldBe(expectedIcon);
    }

    [Fact]
    public void FormatNextOccurrence_ShouldFormatDateInGerman()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Hochzeit", new DateOnly(2026, 6, 20), false, 1);

        // Act
        var text = occasion.FormatNextOccurrence(Today);

        // Assert
        text.ShouldBe("20. Juni 2026");
    }

    [Fact]
    public void FormatCountdown_ShouldReturnVergangen_WhenOccasionIsInThePast()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Studienabschluss", new DateOnly(2026, 5, 1), false, 1);

        // Act
        var countdown = occasion.FormatCountdown(Today);

        // Assert
        countdown.ShouldBe("vergangen");
    }

    [Fact]
    public void FormatCountdown_ShouldReturnHeute_WhenOccasionIsToday()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Umzug", Today, false, 1);

        // Act
        var countdown = occasion.FormatCountdown(Today);

        // Assert
        countdown.ShouldBe("heute");
    }

    [Fact]
    public void FormatCountdown_ShouldReturnMorgen_WhenOccasionIsTomorrow()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Umzug", Today.AddDays(1), false, 1);

        // Act
        var countdown = occasion.FormatCountdown(Today);

        // Assert
        countdown.ShouldBe("morgen");
    }

    [Fact]
    public void FormatCountdown_ShouldReturnDaysUntilOccurrence_WhenOccasionIsFurtherInTheFuture()
    {
        // Arrange
        var occasion = new Occasion(OccasionType.Custom, "Umzug", Today.AddDays(5), false, 1);

        // Act
        var countdown = occasion.FormatCountdown(Today);

        // Assert
        countdown.ShouldBe("in 5 Tagen");
    }
}
