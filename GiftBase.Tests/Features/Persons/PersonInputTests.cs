using GiftBase.Features.Persons;
using GiftBase.Tests.Helper;
using Shouldly;

namespace GiftBase.Tests.Features.Persons;

public class PersonInputTests
{
    [Fact]
    public void PersonInput_ShouldBeCreated()
    {
        // Arrange
        var personInput = new PersonInput
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.Parse("1990-01-01"),
            Relation = Core.Enums.Relation.Friend,
            NotificationsEnabled = false
        };

        // Assert
        personInput.FirstName.ShouldBe("John");
        personInput.LastName.ShouldBe("Doe");
        personInput.DateOfBirth.ShouldBe(DateTime.Parse("1990-01-01"));
        personInput.Relation.ShouldBe(Core.Enums.Relation.Friend);
        personInput.NotificationsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void PersonInput_ShouldDefaultNotificationsEnabledToTrue()
    {
        // Arrange
        var personInput = new PersonInput
        {
            FirstName = "John",
            LastName = "Doe",
            Relation = Core.Enums.Relation.Friend
        };

        // Assert
        personInput.NotificationsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void PersonInput_Validate_Should_Fail_WhenFirstNameIsMissing()
    {
        // Arrange
        var personInput = new PersonInput
        {
            LastName = "Doe",
            DateOfBirth = DateTime.Parse("1990-01-01"),
            Relation = Core.Enums.Relation.Friend
        };

        // Act
        var result = ValidationHelper.Validate(personInput);

        // Assert
        result.ShouldContain(r => r.MemberNames.Contains(nameof(PersonInput.FirstName)));
        result.ShouldContain(r => r.ErrorMessage == "Vorname ist erforderlich.");
    }

    [Fact]
    public void PersonInput_Validate_Should_Fail_WhenLastNameIsMissing()
    {
        // Arrange
        var personInput = new PersonInput
        {
            FirstName = "John",
            DateOfBirth = DateTime.Parse("1990-01-01"),
            Relation = Core.Enums.Relation.Friend
        };

        // Act
        var result = ValidationHelper.Validate(personInput);

        // Assert
        result.ShouldContain(r => r.MemberNames.Contains(nameof(PersonInput.LastName)));
        result.ShouldContain(r => r.ErrorMessage == "Nachname ist erforderlich.");
    }

    [Fact]
    public void PersonInput_Validate_Should_Fail_WhenFirstNameIsTooLong()
    {
        // Arrange
        var personInput = new PersonInput
        {
            FirstName = new string('A', 101),
            LastName = "Doe",
            DateOfBirth = DateTime.Parse("1990-01-01")
        };

        // Act
        var result = ValidationHelper.Validate(personInput);

        // Assert
        result.ShouldContain(r => r.MemberNames.Contains(nameof(PersonInput.FirstName)));
        result.ShouldContain(r => r.ErrorMessage == "Vorname darf maximal 100 Zeichen lang sein.");
    }

    [Fact]
    public void PersonInput_Validate_Should_Fail_WhenLastNameIsTooLong()
    {
        // Arrange
        var personInput = new PersonInput
        {
            FirstName = "John",
            LastName = new string('B', 101),
            DateOfBirth = DateTime.Parse("1990-01-01")
        };

        // Act
        var result = ValidationHelper.Validate(personInput);

        // Assert
        result.ShouldContain(r => r.MemberNames.Contains(nameof(PersonInput.LastName)));
        result.ShouldContain(r => r.ErrorMessage == "Nachname darf maximal 100 Zeichen lang sein.");
    }

    [Fact]
    public void PersonInput_Validate_Should_Fail_WhenInterestsAreTooLong()
    {
        // Arrange
        var personInput = new PersonInput
        {
            FirstName = "John",
            LastName = "Doe",
            Interests = new string('C', 201)
        };

        // Act
        var result = ValidationHelper.Validate(personInput);

        // Assert
        result.ShouldContain(r => r.MemberNames.Contains(nameof(PersonInput.Interests)));
        result.ShouldContain(r => r.ErrorMessage == "Interessen dürfen maximal 200 Zeichen lang sein.");
    }
}