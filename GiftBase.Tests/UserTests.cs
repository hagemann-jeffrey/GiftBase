using GiftBase.Core.Entities;
using Shouldly;

namespace GiftBase.Tests;

public class UserTests
{
    [Fact]
    public void User_ShouldBeCreated()
    {
        // Act
        var user = new User("email", "token", DateTime.Parse("2024-01-01"));

        // Assert
        user.Email.ShouldBe("email");
        user.IsEmailVerified.ShouldBeFalse();
        user.VerificationToken.ShouldBe("token");
        user.TokenExpiresAt.ShouldBe(DateTime.Parse("2024-01-01"));
    }

    [Fact]
    public void User_ShouldConfirmEmail()
    {
        // Arrange
        var user = new User("email", "token", DateTime.Parse("2024-01-01"));

        // Act
        user.ConfirmEmail();

        // Assert
        user.IsEmailVerified.ShouldBeTrue();
        user.VerificationToken.ShouldBeNull();
        user.TokenExpiresAt.ShouldBeNull();
    }

    [Fact]
    public void User_ShouldSetPasswordHash()
    {
        // Arrange
        var user = new User("email");
        var passwordHash = "passwordHash";

        // Act
        user.SetPasswordHash(passwordHash);

        // Assert
        user.PasswordHash.ShouldBe(passwordHash);
    }
}
