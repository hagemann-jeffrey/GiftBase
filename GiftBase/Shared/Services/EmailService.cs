using GiftBase.Core.Interfaces;
using GiftBase.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GiftBase.Shared.Services;

public class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var settings = options.Value;

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));

        email.To.Add(MailboxAddress.Parse(to));

        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(settings.SmtpServer, settings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword);
            await smtp.SendAsync(email);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}
