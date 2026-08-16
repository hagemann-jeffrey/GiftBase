using System.Security.Cryptography;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Features.User;

public class AuthService(IDbContextFactory<GiftBaseDbContext> dbContextFactory, ILogger<AuthService> logger, IPasswordHasher<Core.Entities.User> passwordHasher) : IAuthService
{
    public async Task<bool> RegisterUserAsync(string email, string password)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (await dbContext.Users.AnyAsync(u => u.Email == email))
            return false;

        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        var user = new Core.Entities.User(email, token, DateTime.UtcNow.AddHours(24));

        user.SetPasswordHash(passwordHasher.HashPassword(user, password));

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // TODO: Implement email sending logic
        logger.LogInformation("User registered with email: {Email}. Verification token: {Token}", email, token);

        return true;
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null)
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
