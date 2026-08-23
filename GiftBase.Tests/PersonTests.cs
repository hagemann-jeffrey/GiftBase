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
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Relation.Friend, 1);

        // Assert
        person.FirstName.ShouldBe("John");
        person.LastName.ShouldBe("Doe");
        person.DateOfBirth.ShouldBe(DateOnly.Parse("1990-01-01"));
        person.Relation.ShouldBe(Relation.Friend);
        person.UserId.ShouldBe(1);
    }

    [Fact]
    public void Person_ShouldBeUpdated()
    {
        // Arrange
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Relation.Friend, 1);
        var updateDto = new Core.Dtos.PersonUpdateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.Parse("1992-02-02"),
            Relation = Relation.Family
        };

        // Act
        person.Update(updateDto);

        // Assert
        person.FirstName.ShouldBe("Jane");
        person.LastName.ShouldBe("Smith");
        person.DateOfBirth.ShouldBe(DateOnly.Parse("1992-02-02"));
        person.Relation.ShouldBe(Relation.Family);
    }
}
