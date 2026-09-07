using GiftBase.Core.Enums;
using GiftBase.Features.Occasions;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests;

public class OccasionInputTests
{
    private static OccasionInput ValidCustomOccasionInput() => new()
    {
        Type = OccasionType.Custom,
        Title = "Hochzeit",
        Date = new DateTime(2027, 6, 5),
        IsRecurring = false
    };

    [Fact]
    public void OccasionInput_Validate_Should_Pass_WhenAllValuesAreValid()
    {
        // Arrange
        var occasionInput = ValidCustomOccasionInput();

        // Act
        var result = ValidationHelper.Validate(occasionInput);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void OccasionInput_Validate_Should_Fail_WhenCustomTitleIsMissing()
    {
        // Arrange
        var occasionInput = ValidCustomOccasionInput();
        occasionInput.Title = "   ";

        // Act
        var result = ValidationHelper.Validate(occasionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Titel ist erforderlich.");
    }

    [Fact]
    public void OccasionInput_Validate_Should_Fail_WhenCustomTitleIsTooLong()
    {
        // Arrange
        var occasionInput = ValidCustomOccasionInput();
        occasionInput.Title = new string('a', 101);

        // Act
        var result = ValidationHelper.Validate(occasionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Titel darf maximal 100 Zeichen lang sein.");
    }

    [Fact]
    public void OccasionInput_Validate_Should_Fail_WhenCustomDateIsMissing()
    {
        // Arrange
        var occasionInput = ValidCustomOccasionInput();
        occasionInput.Date = null;

        // Act
        var result = ValidationHelper.Validate(occasionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Datum ist erforderlich.");
    }

    [Fact]
    public void OccasionInput_Validate_Should_Pass_WhenFixedOccasionHasNoTitle()
    {
        // Arrange
        var occasionInput = new OccasionInput
        {
            Type = OccasionType.Christmas,
            Title = null,
            Date = new DateTime(2026, 12, 24),
            IsRecurring = true
        };

        // Act
        var result = ValidationHelper.Validate(occasionInput);

        // Assert
        result.ShouldBeEmpty();
    }
}
