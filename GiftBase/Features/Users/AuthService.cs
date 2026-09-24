using GiftBase.Core.Entities;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using GiftBase.Features.Users.Register;
using GiftBase.Shared.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.Users;

public class AuthService(IDbContextFactory<GiftBaseDbContext> dbContextFactory, ILogger<AuthService> logger,
IPasswordHasher<User> passwordHasher, IEmailService emailService,
NavigationManager navigationManager) : IAuthService
{
    public async Task RegisterUserAsync(string email, string password)
    {
        if (!RegistrationDomains.IsAllowed(email))
        {
            logger.LogWarning("Attempted registration with non-IU email: {Email}", email);

            throw new ConflictException("Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (await dbContext.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException("Diese E-Mail-Adresse ist bereits registriert.");
        }

        string token = TokenGenerator.Generate();

        var user = new User(email, token, DateTime.UtcNow.AddHours(24));

        user.SetPasswordHash(passwordHasher.HashPassword(user, password));

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var confirmationLink = $"{navigationManager.BaseUri}confirmemail?Token={token}";
        var (subject, body) = RegistrationEmailBuilder.Build(confirmationLink);

        await emailService.SendEmailAsync(email, subject, body);
    }

    public async Task ConfirmEmailAsync(string token)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token)
            ?? throw new NotFoundException("Der Bestätigungslink ist ungültig.");

        if (user.TokenExpiresAt < DateTime.UtcNow)
        {
            throw new ConflictException("Der Bestätigungslink ist abgelaufen.");
        }

        user.ConfirmEmail();

        await dbContext.SaveChangesAsync();
    }

    public async Task<int?> LoginUserAsync(string email, string password)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !user.IsEmailVerified)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Failed login attempt for email: {Email}", email);

            return null;
        }

        return user.Id;
    }
}
