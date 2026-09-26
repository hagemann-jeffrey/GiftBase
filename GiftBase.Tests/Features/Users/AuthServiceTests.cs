using GiftBase.Core.Entities;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Data;
using GiftBase.Features.Users;
using GiftBase.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace GiftBase.Tests.Features.Users;

public class AuthServiceTests
{
    private const string BaseUrl = "https://giftbase.example";

    private readonly TestDbContextFactory _dbContextFactory;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _passwordHasher = Substitute.For<IPasswordHasher<User>>();
        _emailService = Substitute.For<IEmailService>();
        var logger = Substitute.For<ILogger<AuthService>>();
        _authService = new AuthService(_dbContextFactory, logger, _passwordHasher, _emailService);
    }

    // RegisterUserAsync

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowConflict_WhenEmailIsNotIU()
    {
        // Act
        var action = () => _authService.RegisterUserAsync("test@gmail.com", "Password123!", BaseUrl);

        // Assert
        await Should.ThrowAsync<ConflictException>(action);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var existingUser = new User("user@iu-study.org");
        existingUser.SetPasswordHash("existing_hash");
        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        // Act
        var action = () => _authService.RegisterUserAsync("user@iu-study.org", "Password123!", BaseUrl);

        // Assert
        await Should.ThrowAsync<ConflictException>(action);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WithIuStudyEmail()
    {
        // Act
        await _authService.RegisterUserAsync("user@iu-study.org", "Password123!", BaseUrl);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == "user@iu-study.org");
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WithIuOrgEmail()
    {
        // Act
        await _authService.RegisterUserAsync("user@iu.org", "Password123!", BaseUrl);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == "user@iu.org");
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldHashPassword()
    {
        // Arrange
        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>()).Returns("hashed_password");

        // Act
        await _authService.RegisterUserAsync("user@iu-study.org", "Password123!", BaseUrl);

        // Assert
        _passwordHasher.Received(1).HashPassword(Arg.Any<User>(), "Password123!");
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldSendConfirmationEmail()
    {
        // Act
        await _authService.RegisterUserAsync("user@iu-study.org", "Password123!", BaseUrl);

        // Assert
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.SingleAsync(u => u.Email == "user@iu-study.org");
        var confirmationLink = $"{BaseUrl}/confirmemail?Token={user.VerificationToken}";

        await _emailService.Received(1).SendEmailAsync(
            Arg.Is("user@iu-study.org"),
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains(confirmationLink)));
    }

    // ConfirmEmailAsync

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrowNotFound_WhenTokenNotFound()
    {
        // Act
        var action = () => _authService.ConfirmEmailAsync("nonexistent-token");

        // Assert
        await Should.ThrowAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrowConflict_WhenTokenExpired()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = new User("user@iu-study.org", "expired-token", DateTime.UtcNow.AddHours(-1));
        user.SetPasswordHash("hashed_password");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var action = () => _authService.ConfirmEmailAsync("expired-token");

        // Assert
        await Should.ThrowAsync<ConflictException>(action);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldConfirm_WhenValidToken()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = new User("user@iu-study.org", "valid-token", DateTime.UtcNow.AddHours(24));
        user.SetPasswordHash("hashed_password");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        await _authService.ConfirmEmailAsync("valid-token");

        // Assert
        await using var verifyContext = _dbContextFactory.CreateDbContext();
        var confirmedUser = await verifyContext.Users.FindAsync(user.Id);
        confirmedUser.ShouldNotBeNull();
        confirmedUser.IsEmailVerified.ShouldBeTrue();
        confirmedUser.VerificationToken.ShouldBeNull();
        confirmedUser.TokenExpiresAt.ShouldBeNull();
    }

    // LoginUserAsync

    [Fact]
    public async Task LoginUserAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Act
        var result = await _authService.LoginUserAsync("nonexistent@iu-study.org", "Password123!");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnNull_WhenEmailNotVerified()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = new User("user@iu-study.org");
        user.SetPasswordHash("hashed_password");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _authService.LoginUserAsync("user@iu-study.org", "Password123!");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnNull_WhenPasswordWrong()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = new User("user@iu-study.org", "token", DateTime.UtcNow.AddHours(24));
        user.SetPasswordHash("hashed_password");
        user.ConfirmEmail();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "hashed_password", "WrongPassword")
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await _authService.LoginUserAsync("user@iu-study.org", "WrongPassword");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnUserId_WhenCredentialsCorrect()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var user = new User("user@iu-study.org", "token", DateTime.UtcNow.AddHours(24));
        user.SetPasswordHash("hashed_password");
        user.ConfirmEmail();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "hashed_password", "CorrectPassword")
            .Returns(PasswordVerificationResult.Success);

        // Act
        var result = await _authService.LoginUserAsync("user@iu-study.org", "CorrectPassword");

        // Assert
        result.ShouldBe(user.Id);
    }
}
