using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using Shouldly;

namespace GiftBase.Tests;

public class PersonTests
{
    [Fact]
    public void Person_ShouldBeCreated()
    {
        // Act
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Relation.Friend, 1, "Bücher, Pflanzen");

        // Assert
        person.FirstName.ShouldBe("John");
        person.LastName.ShouldBe("Doe");
        person.DateOfBirth.ShouldBe(DateOnly.Parse("1990-01-01"));
        person.Relation.ShouldBe(Relation.Friend);
        person.UserId.ShouldBe(1);
        person.Interests.ShouldBe("Bücher, Pflanzen");
        person.NotificationsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Person_ShouldBeUpdated()
    {
        // Arrange
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Relation.Friend, 1, "Bücher");
        var updateDto = new Core.Dtos.Persons.PersonUpdateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.Parse("1992-02-02"),
            Relation = Relation.Family,
            Interests = "Pflanzen, Kochen",
            NotificationsEnabled = false
        };

        // Act
        person.Update(updateDto);

        // Assert
        person.FirstName.ShouldBe("Jane");
        person.LastName.ShouldBe("Smith");
        person.DateOfBirth.ShouldBe(DateOnly.Parse("1992-02-02"));
        person.Relation.ShouldBe(Relation.Family);
        person.Interests.ShouldBe("Pflanzen, Kochen");
        person.NotificationsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void GetAge_ShouldReturnCompletedYears_WhenBirthdayHasAlreadyOccurredThisYear()
    {
        // Arrange
        var person = new Person("John", "Doe", new DateOnly(1992, 1, 1), Relation.Friend, 1);

        // Act
        var age = person.GetAge(new DateOnly(2026, 9, 13));

        // Assert
        age.ShouldBe(34);
    }

    [Fact]
    public void GetAge_ShouldNotCountThisYear_WhenBirthdayHasNotOccurredYet()
    {
        // Arrange
        var person = new Person("John", "Doe", new DateOnly(1992, 12, 24), Relation.Friend, 1);

        // Act
        var age = person.GetAge(new DateOnly(2026, 9, 13));

        // Assert
        age.ShouldBe(33);
    }

    [Fact]
    public void GetAge_ShouldCountBirthday_WhenTodayIsTheBirthday()
    {
        // Arrange
        var person = new Person("John", "Doe", new DateOnly(1992, 9, 13), Relation.Friend, 1);

        // Act
        var age = person.GetAge(new DateOnly(2026, 9, 13));

        // Assert
        age.ShouldBe(34);
    }

    [Fact]
    public void GetAge_ShouldReturnNull_WhenDateOfBirthIsNotSet()
    {
        // Arrange
        var person = new Person("John", "Doe", null, Relation.Friend, 1);

        // Act
        var age = person.GetAge(new DateOnly(2026, 9, 13));

        // Assert
        age.ShouldBeNull();
    }
}
