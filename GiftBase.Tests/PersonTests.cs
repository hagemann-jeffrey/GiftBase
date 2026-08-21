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
}
