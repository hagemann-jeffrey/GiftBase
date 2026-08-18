using System.ComponentModel.DataAnnotations;
using GiftBase.Validation;
using Shouldly;

namespace GiftBase.Tests;

public class IuEmailAttributeTests
{
    private readonly IuEmailAttribute _iuEmailAttribute = new IuEmailAttribute();

    [Theory]
    [InlineData("test@iu-study.org")]
    [InlineData("test@iu.org")]
    public void ValidIuEmail_ShouldPassValidation(string validEmail)
    {
        // Arrange
        var validationContext = new ValidationContext(new { Email = validEmail });

        // Act
        var result = _iuEmailAttribute.GetValidationResult(validEmail, validationContext);

        // Assert
        result.ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("test@gmail.com")]
    [InlineData("test@outlook.com")]
    public void InvalidIuEmail_ShouldFailValidation(string invalidEmail)
    {
        // Arrange
        var validationContext = new ValidationContext(new { Email = invalidEmail });

        // Act
        var result = _iuEmailAttribute.GetValidationResult(invalidEmail, validationContext);

        // Assert
        result.ShouldNotBe(ValidationResult.Success);
        result?.ErrorMessage.ShouldBe("Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.");
    }
}
