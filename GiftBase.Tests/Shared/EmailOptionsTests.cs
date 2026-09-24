using System.ComponentModel.DataAnnotations;
using GiftBase.Options;
using Shouldly;

namespace GiftBase.Tests.Shared;

public class EmailOptionsTests
{
    private static EmailOptions BuildOptions(
        string smtpServer = "smtp.example.com",
        int smtpPort = 587,
        string smtpUsername = "user",
        string smtpPassword = "password",
        string senderEmail = "no-reply@example.com") => new()
        {
            SmtpServer = smtpServer,
            SmtpPort = smtpPort,
            SmtpUsername = smtpUsername,
            SmtpPassword = smtpPassword,
            SenderEmail = senderEmail
        };

    private static List<ValidationResult> Validate(EmailOptions options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);

        return results;
    }

    [Fact]
    public void Validation_ShouldFailWithSmtpServerMessage_WhenSmtpServerIsMissing()
    {
        // Arrange
        var options = BuildOptions(smtpServer: "");

        // Act
        var results = Validate(options);

        // Assert
        results.ShouldContain(r => r.ErrorMessage == "EmailSettings:SmtpServer ist nicht konfiguriert.");
    }

    [Fact]
    public void Validation_ShouldFailWithSmtpPortMessage_WhenSmtpPortIsOutOfRange()
    {
        // Arrange
        var options = BuildOptions(smtpPort: 0);

        // Act
        var results = Validate(options);

        // Assert
        results.ShouldContain(r => r.ErrorMessage == "EmailSettings:SmtpPort ist keine gültige Portnummer.");
    }

    [Fact]
    public void Validation_ShouldFailWithSenderEmailMessage_WhenSenderEmailIsMissing()
    {
        // Arrange
        var options = BuildOptions(senderEmail: "");

        // Act
        var results = Validate(options);

        // Assert
        results.ShouldContain(r => r.ErrorMessage == "EmailSettings:SenderEmail ist nicht konfiguriert.");
    }

    [Fact]
    public void Validation_ShouldPass_WhenAllSettingsArePresent()
    {
        // Arrange
        var options = BuildOptions();

        // Act
        var results = Validate(options);

        // Assert
        results.ShouldBeEmpty();
    }
}
