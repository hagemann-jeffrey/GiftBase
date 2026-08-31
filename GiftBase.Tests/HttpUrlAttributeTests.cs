using System.ComponentModel.DataAnnotations;
using GiftBase.Shared.Validation;
using Shouldly;

namespace GiftBase.Tests;

public class HttpUrlAttributeTests
{
    private readonly HttpUrlAttribute _httpUrlAttribute = new HttpUrlAttribute();

    [Theory]
    [InlineData("https://amazon.de/produkt/123")]
    [InlineData("http://example.de")]
    public void ValidHttpUrl_ShouldPassValidation(string validLink)
    {
        // Arrange
        var validationContext = new ValidationContext(new { Link = validLink });

        // Act
        var result = _httpUrlAttribute.GetValidationResult(validLink, validationContext);

        // Assert
        result.ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyLink_ShouldPassValidation(string? emptyLink)
    {
        // Arrange
        var validationContext = new ValidationContext(new { Link = emptyLink });

        // Act
        var result = _httpUrlAttribute.GetValidationResult(emptyLink, validationContext);

        // Assert
        result.ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("www.amazon.de")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.de")]
    [InlineData("kein link")]
    public void InvalidHttpUrl_ShouldFailValidation(string invalidLink)
    {
        // Arrange
        var validationContext = new ValidationContext(new { Link = invalidLink });

        // Act
        var result = _httpUrlAttribute.GetValidationResult(invalidLink, validationContext);

        // Assert
        result.ShouldNotBe(ValidationResult.Success);
        result?.ErrorMessage.ShouldBe("Bitte gib einen gültigen Link an, der mit http:// oder https:// beginnt.");
    }
}