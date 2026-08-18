using GiftBase.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GiftBase.Features.User;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(configuration["EmailSettings:SenderName"], configuration["EmailSettings:SenderEmail"]));

        email.To.Add(MailboxAddress.Parse(to));

        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            smtp.Connect(configuration["EmailSettings:SmtpServer"], int.Parse(configuration["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
            smtp.Authenticate(configuration["EmailSettings:SmtpUsername"], configuration["EmailSettings:SmtpPassword"]);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email.");
        }
        finally
        {
            smtp.Disconnect(true);
        }
    }
}