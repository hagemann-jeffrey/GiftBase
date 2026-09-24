using GiftBase.Shared.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;

namespace GiftBase.Tests.Shared;

public class EmailServiceTests
{
    private static IConfiguration BuildConfiguration(
        string? smtpServer = "smtp.example.com",
        string? smtpPort = "587",
        string? smtpUsername = "user",
        string? smtpPassword = "password",
        string? senderEmail = "no-reply@example.com")
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration["EmailSettings:SmtpServer"].Returns(smtpServer);
        configuration["EmailSettings:SmtpPort"].Returns(smtpPort);
        configuration["EmailSettings:SmtpUsername"].Returns(smtpUsername);
        configuration["EmailSettings:SmtpPassword"].Returns(smtpPassword);
        configuration["EmailSettings:SenderEmail"].Returns(senderEmail);
        return configuration;
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowWithSmtpServerMessage_WhenSmtpServerIsMissing()
    {
        // Arrange
        var emailService = new EmailService(BuildConfiguration(smtpServer: ""));

        // Act
        var action = () => emailService.SendEmailAsync("test@example.com", "Betreff", "Text");

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        exception.Message.ShouldBe("EmailSettings:SmtpServer ist nicht konfiguriert.");
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowWithSmtpPortMessage_WhenSmtpPortIsNotANumber()
    {
        // Arrange
        var emailService = new EmailService(BuildConfiguration(smtpPort: "not-a-number"));

        // Act
        var action = () => emailService.SendEmailAsync("test@example.com", "Betreff", "Text");

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        exception.Message.ShouldBe("EmailSettings:SmtpPort ist keine gültige Zahl.");
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowWithSenderEmailMessage_WhenSenderEmailIsMissing()
    {
        // Arrange
        var emailService = new EmailService(BuildConfiguration(senderEmail: ""));

        // Act
        var action = () => emailService.SendEmailAsync("test@example.com", "Betreff", "Text");

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        exception.Message.ShouldBe("EmailSettings:SenderEmail ist nicht konfiguriert.");
    }
}
