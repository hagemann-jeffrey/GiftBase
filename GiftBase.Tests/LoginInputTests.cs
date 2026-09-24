using GiftBase.Features.Users.Login;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests;

public class LoginInputTests
{
    [Fact]
    public void LoginInput_ShouldBeCreated()
    {
        // Act
        var loginInput = new LoginInput { Email = "email", Password = "password" };

        // Assert
        loginInput.Email.ShouldBe("email");
        loginInput.Password.ShouldBe("password");
    }

    [Fact]
    public void LoginInput_Validate_ShouldSucceed()
    {
        // Arrange
        var loginInput = new LoginInput { Email = "email@example.com", Password = "password" };

        // Act
        var results = ValidationHelper.Validate(loginInput);

        // Assert
        results.ShouldBeEmpty();
    }

    [Fact]
    public void LoginInput_Validate_ShouldFail_WhenEmailIsMissing()
    {
        // Arrange
        var loginInput = new LoginInput { Password = "password" };

        // Act
        var results = ValidationHelper.Validate(loginInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(LoginInput.Email)));
        results.ShouldContain(r => r.ErrorMessage == "Bitte gib deine E-Mail-Adresse ein.");
    }

    [Fact]
    public void LoginInput_Validate_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var loginInput = new LoginInput { Email = "invalid-email", Password = "password" };

        // Act
        var results = ValidationHelper.Validate(loginInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(LoginInput.Email)));
        results.ShouldContain(r => r.ErrorMessage == "Das ist keine gültige E-Mail.");
    }

    [Fact]
    public void LoginInput_Validate_ShouldFail_WhenPasswordIsMissing()
    {
        // Arrange
        var loginInput = new LoginInput { Email = "email" };

        // Act
        var results = ValidationHelper.Validate(loginInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(LoginInput.Password)));
        results.ShouldContain(r => r.ErrorMessage == "Bitte gib dein Passwort ein.");
    }
}
