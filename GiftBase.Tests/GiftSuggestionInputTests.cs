using GiftBase.Features.GiftSuggestions;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests;

public class GiftSuggestionInputTests
{
    private static GiftSuggestionInput ValidGiftSuggestionInput() => new()
    {
        OccasionId = 1,
        Budget = 50,
        AdditionalHint = "Etwas Persönliches",
        ConsentGiven = true
    };

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Pass_WhenAllValuesAreValid()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Pass_WhenHintIsMissing()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();
        giftSuggestionInput.AdditionalHint = null;

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Fail_WhenOccasionIsMissing()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();
        giftSuggestionInput.OccasionId = null;

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Anlass ist erforderlich.");
    }

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Fail_WhenBudgetIsMissing()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();
        giftSuggestionInput.Budget = null;

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Budget ist erforderlich.");
    }

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Fail_WhenBudgetIsZeroOrNegative()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();
        giftSuggestionInput.Budget = 0;

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Budget muss zwischen 1 und 99.999.999 liegen.");
    }

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Fail_WhenHintIsTooLong()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();
        giftSuggestionInput.AdditionalHint = new string('a', 501);

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Hinweis darf maximal 500 Zeichen lang sein.");
    }

    [Fact]
    public void GiftSuggestionInput_Validate_Should_Fail_WhenConsentIsNotGiven()
    {
        // Arrange
        var giftSuggestionInput = ValidGiftSuggestionInput();
        giftSuggestionInput.ConsentGiven = false;

        // Act
        var result = ValidationHelper.Validate(giftSuggestionInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Bitte bestätige den Hinweis zur Datenübertragung.");
    }
}
