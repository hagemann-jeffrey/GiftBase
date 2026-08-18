using GiftBase.Features.User.Register;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests;

public class RegisterInputTests
{
    [Fact]
    public void RegisterInput_ShouldBeCreated()
    {
        // Act
        var registerInput = new RegisterInput { Email = "email", Password = "password", ConfirmPassword = "password" };

        // Assert
        registerInput.Email.ShouldBe("email");
        registerInput.Password.ShouldBe("password");
        registerInput.ConfirmPassword.ShouldBe("password");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldSucceed()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "email@iu.org",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldBeEmpty();
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenEmailIsMissing()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
        results.ShouldContain(r => r.ErrorMessage == "Bitte gib deine E-Mail-Adresse ein.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "invalid-email",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
        results.ShouldContain(r => r.ErrorMessage == "Das ist keine gültige E-Mail.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenEmailIsTooLong()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = new string('a', 101) + "@iu.org",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
        results.ShouldContain(r => r.ErrorMessage == "Die E-Mail darf maximal 100 Zeichen lang sein.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenEmailIsNoIuEmail()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "email@example.com",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
        results.ShouldContain(r => r.ErrorMessage == "Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenPasswordIsTooShort()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "email@iu.org",
            Password = "short",
            ConfirmPassword = "short"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.Password)));
        results.ShouldContain(r => r.ErrorMessage == "Das Passwort muss mindestens 8 Zeichen lang sein.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenPasswordIsMissing()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "email@iu.org",
            ConfirmPassword = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.Password)));
        results.ShouldContain(r => r.ErrorMessage == "Bitte gib dein Passwort ein.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenConfirmPasswordIsMissing()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "email@iu.org",
            Password = "password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.ConfirmPassword)));
        results.ShouldContain(r => r.ErrorMessage == "Bitte bestätige dein Passwort.");
    }

    [Fact]
    public void RegisterInput_Validate_ShouldFail_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var registerInput = new RegisterInput
        {
            Email = "email@iu.org",
            Password = "password",
            ConfirmPassword = "wrong-password"
        };

        // Act
        var results = ValidationHelper.Validate(registerInput);

        // Assert
        results.ShouldContain(r => r.MemberNames.Contains(nameof(RegisterInput.ConfirmPassword)));
        results.ShouldContain(r => r.ErrorMessage == "Die Passwörter stimmen nicht überein.");
    }
}
