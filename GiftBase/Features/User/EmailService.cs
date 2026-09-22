using GiftBase.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GiftBase.Features.User;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = RequireSetting(configuration, "EmailSettings:SmtpServer");
        var smtpPortSetting = RequireSetting(configuration, "EmailSettings:SmtpPort");
        var smtpUsername = RequireSetting(configuration, "EmailSettings:SmtpUsername");
        var smtpPassword = RequireSetting(configuration, "EmailSettings:SmtpPassword");
        var senderEmail = RequireSetting(configuration, "EmailSettings:SenderEmail");

        if (!int.TryParse(smtpPortSetting, out var smtpPort))
        {
            throw new InvalidOperationException("EmailSettings:SmtpPort ist keine gültige Zahl.");
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(configuration["EmailSettings:SenderName"], senderEmail));

        email.To.Add(MailboxAddress.Parse(to));

        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
            await smtp.SendAsync(email);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }

    private static string RequireSetting(IConfiguration configuration, string key)
    {
        var value = configuration[key];

        return !string.IsNullOrWhiteSpace(value) ? value : throw new InvalidOperationException($"{key} ist nicht konfiguriert.");
    }
}
