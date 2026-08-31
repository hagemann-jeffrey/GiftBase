using GiftBase.Features.Gifts;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests;

public class GiftInputTests
{
    private static GiftInput ValidGiftInput() => new()
    {
        Title = "Kaffeemaschine",
        Note = "Am liebsten in Schwarz",
        Link = "https://example.com/kaffee",
        Price = 129.99m,
        Status = Core.Enums.GiftStatus.Idea
    };

    [Fact]
    public void GiftInput_Validate_Should_Pass_WhenAllValuesAreValid()
    {
        // Arrange
        var giftInput = ValidGiftInput();

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GiftInput_Validate_Should_Pass_WhenOptionalValuesAreMissing()
    {
        // Arrange
        var giftInput = new GiftInput { Title = "Gutschein" };

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GiftInput_Validate_Should_Fail_WhenTitleIsMissing()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Title = null!;

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Titel ist erforderlich.");
    }

    [Fact]
    public void GiftInput_Validate_Should_Fail_WhenTitleIsTooLong()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Title = new string('a', 101);

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Titel darf maximal 100 Zeichen lang sein.");
    }

    [Fact]
    public void GiftInput_Validate_Should_Fail_WhenNoteIsTooLong()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Note = new string('a', 1001);

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Notiz darf maximal 1000 Zeichen lang sein.");
    }

    [Fact]
    public void GiftInput_Validate_Should_Fail_WhenLinkIsTooLong()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Link = "https://example.com/" + new string('a', 2000);

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Link darf maximal 2000 Zeichen lang sein.");
    }

    [Fact]
    public void GiftInput_Validate_Should_Fail_WhenLinkIsNotAnHttpUrl()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Link = "www.amazon.de";

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Bitte gib einen gültigen Link an, der mit http:// oder https:// beginnt.");
    }

    [Fact]
    public void GiftInput_Validate_Should_Pass_WhenLinkIsEmpty()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Link = string.Empty;

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GiftInput_Validate_Should_Fail_WhenPriceIsNegative()
    {
        // Arrange
        var giftInput = ValidGiftInput();
        giftInput.Price = -1m;

        // Act
        var result = ValidationHelper.Validate(giftInput);

        // Assert
        result.ShouldContain(r => r.ErrorMessage == "Preis muss zwischen 0 und 99.999.999 liegen.");
    }
}