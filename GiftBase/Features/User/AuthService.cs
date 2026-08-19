using System.Security.Cryptography;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.User;

public class AuthService(IDbContextFactory<GiftBaseDbContext> dbContextFactory, ILogger<AuthService> logger,
IPasswordHasher<Core.Entities.User> passwordHasher, IEmailService emailService,
NavigationManager navigationManager) : IAuthService
{
    public async Task<bool> RegisterUserAsync(string email, string password)
    {
        var isStudentOrLecturer = email.EndsWith("@iu-study.org", StringComparison.OrdinalIgnoreCase) ||
                                email.EndsWith("@iu.org", StringComparison.OrdinalIgnoreCase);

        if (!isStudentOrLecturer)
        {
            logger.LogWarning("Attempted registration with non-IU email: {Email}", email);
            return false;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (await dbContext.Users.AnyAsync(u => u.Email == email))
            return false;

        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        var user = new Core.Entities.User(email, token, DateTime.UtcNow.AddHours(24));

        user.SetPasswordHash(passwordHasher.HashPassword(user, password));

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var confirmationLink = $"{navigationManager.BaseUri}confirmemail?Token={token}";

        var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2>Willkommen bei GiftBase!</h2>
                    <p>Schön, dass du dabei bist. Bitte bestätige deine E-Mail-Adresse, um deinen Account zu aktivieren.</p>
                    <p style='margin-top: 20px;'>
                        <a href='{confirmationLink}' 
                           style='background-color: #594AE2; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                           E-Mail bestätigen
                        </a>
                    </p>
                    <p style='margin-top: 30px; font-size: 12px; color: #888;'>
                        Falls der Button nicht funktioniert, kopiere diesen Link in deinen Browser:<br/>
                        {confirmationLink}
                    </p>
                </div>";

        await emailService.SendEmailAsync(email, "Willkommen bei GiftBase! Bitte bestätige deine E-Mail-Adresse", emailBody);

        return true;
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null)
            return false;

        if (user.TokenExpiresAt < DateTime.UtcNow)
            return false;

        user.ConfirmEmail();

        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<int?> LoginUserAsync(string email, string password)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsEmailVerified)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Failed login attempt for email: {Email}", email);

            return null;
        }

        return user.Id;
    }
}
