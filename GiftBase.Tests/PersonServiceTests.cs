using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Core.Exceptions;
using GiftBase.Features.Persons;
using GiftBase.Tests.Helper;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GiftBase.Tests;

public class PersonServiceTests
{
    private readonly TestDbContextFactory _dbContextFactory;
    private readonly PersonService _personService;

    public PersonServiceTests()
    {
        _dbContextFactory = new TestDbContextFactory();
        _personService = new PersonService(_dbContextFactory);
    }

    [Fact]
    public async Task GetPersonsAsync_ShouldReturnPersonsForUser()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var userId = 1;
        dbContext.Persons.Add(new Person("John", "Doe", DateOnly.MinValue, Core.Enums.Relation.Friend, userId));
        dbContext.Persons.Add(new Person("Jane", "Smith", DateOnly.MinValue, Core.Enums.Relation.Friend, userId));
        await dbContext.SaveChangesAsync();

        // Act
        var persons = await _personService.GetPersonsAsync(userId);

        // Assert
        persons.Count.ShouldBe(2);
        persons.All(p => p.UserId == userId).ShouldBeTrue();
    }

    [Fact]
    public async Task GetPersonsAsync_ShouldReturnEmptyList_WhenNoPersonsExistForUser()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var userId = 2;
        dbContext.Persons.Add(new Person("John", "Doe", DateOnly.MinValue, Core.Enums.Relation.Friend, 1));
        dbContext.Persons.Add(new Person("Jane", "Smith", DateOnly.MinValue, Core.Enums.Relation.Friend, 1));

        // Act
        var persons = await _personService.GetPersonsAsync(userId);

        // Assert
        persons.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetPersonsAsync_ShouldReturnEmptyList_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 999;

        // Act
        var persons = await _personService.GetPersonsAsync(userId);

        // Assert
        persons.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetPersonAsync_ShouldReturnPerson()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Core.Enums.Relation.Friend, 1);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act
        var foundPerson = await _personService.GetPersonAsync(person.Id, person.UserId);

        // Assert
        foundPerson.Id.ShouldBe(person.Id);
        foundPerson.FirstName.ShouldBe("John");
        foundPerson.LastName.ShouldBe("Doe");
    }

    [Fact]
    public async Task GetPersonAsync_ShouldThrowNotFoundException_WhenUserIdDoesNotMatch()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Core.Enums.Relation.Friend, 1);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _personService.GetPersonAsync(person.Id, 999));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task GetPersonAsync_ShouldThrowNotFoundException_WhenPersonDoesNotExist()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _personService.GetPersonAsync(999, 1));

        exception.Message.ShouldBe("Person konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task AddPersonAsync_ShouldAddPerson()
    {
        // Arrange
        var personAddDto = new PersonAddDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.Parse("1990-01-01"),
            Relation = Core.Enums.Relation.Friend,
            UserId = 1,
            Interests = "Bücher, Pflanzen"
        };

        // Act
        var addedPerson = await _personService.AddPersonAsync(personAddDto);

        // Assert
        addedPerson.Id.ShouldBeGreaterThan(0);
        addedPerson.FirstName.ShouldBe("John");
        addedPerson.LastName.ShouldBe("Doe");
        addedPerson.DateOfBirth.ShouldBe(DateOnly.Parse("1990-01-01"));
        addedPerson.Relation.ShouldBe(Core.Enums.Relation.Friend);
        addedPerson.UserId.ShouldBe(1);
        addedPerson.Interests.ShouldBe("Bücher, Pflanzen");

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var personInDb = await dbContext.Persons.SingleOrDefaultAsync(p => p.Id == addedPerson.Id);
        personInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldUpdatePerson()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Core.Enums.Relation.Friend, 1, "Bücher");
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var personUpdateDto = new PersonUpdateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.Parse("1992-02-02"),
            Relation = Core.Enums.Relation.Family,
            Interests = "Pflanzen, Kochen"
        };

        // Act
        var updatedPerson = await _personService.UpdatePersonAsync(person.Id, person.UserId, personUpdateDto);

        // Assert
        updatedPerson.FirstName.ShouldBe("Jane");
        updatedPerson.LastName.ShouldBe("Smith");
        updatedPerson.DateOfBirth.ShouldBe(DateOnly.Parse("1992-02-02"));
        updatedPerson.Relation.ShouldBe(Core.Enums.Relation.Family);
        updatedPerson.Interests.ShouldBe("Pflanzen, Kochen");
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldThrowNotFoundException_WhenUserIdDoesNotMatch()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Core.Enums.Relation.Friend, 1);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var personUpdateDto = new PersonUpdateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.Parse("1992-02-02"),
            Relation = Core.Enums.Relation.Family
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _personService.UpdatePersonAsync(person.Id, 999, personUpdateDto));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldThrowException_WhenPersonDoesNotExist()
    {
        // Arrange
        var personUpdateDto = new PersonUpdateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.Parse("1992-02-02"),
            Relation = Core.Enums.Relation.Family
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () =>
            await _personService.UpdatePersonAsync(999, 1, personUpdateDto));

        exception.Message.ShouldBe("Person konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldDeletePerson()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Core.Enums.Relation.Friend, 1);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act
        await _personService.DeletePersonAsync(person.Id, person.UserId);

        // Assert
        var deletedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.Id == person.Id);
        deletedPerson.ShouldBeNull();
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldThrowNotFoundException_WhenUserIdDoesNotMatch()
    {
        // Arrange
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", DateOnly.Parse("1990-01-01"), Core.Enums.Relation.Friend, 1);
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _personService.DeletePersonAsync(person.Id, 999));

        exception.Message.ShouldBe($"Person konnte nicht gefunden werden: {person.Id}");
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldThrowException_WhenPersonDoesNotExist()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () =>
            await _personService.DeletePersonAsync(999, 1));

        exception.Message.ShouldBe("Person konnte nicht gefunden werden: 999");
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldMoveBirthdayOccasion_WhenDateOfBirthChanges()
    {
        // Arrange
        await using var arrangeContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", new DateOnly(1990, 5, 24), Core.Enums.Relation.Friend, 1);
        arrangeContext.Persons.Add(person);
        await arrangeContext.SaveChangesAsync();
        arrangeContext.Occasions.Add(new Occasion(Core.Enums.OccasionType.Birthday, null, new DateOnly(1990, 5, 24), true, person.Id));
        await arrangeContext.SaveChangesAsync();

        // Act
        await _personService.UpdatePersonAsync(person.Id, 1, new PersonUpdateDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1991, 6, 30),
            Relation = Core.Enums.Relation.Friend
        });

        // Assert
        await using var assertContext = _dbContextFactory.CreateDbContext();
        var birthday = await assertContext.Occasions.SingleAsync(o => o.PersonId == person.Id);
        birthday.Date.ShouldBe(new DateOnly(1991, 6, 30));
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldThrowConflictException_WhenDateOfBirthIsRemovedWhileBirthdayOccasionExists()
    {
        // Arrange
        await using var arrangeContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", new DateOnly(1990, 5, 24), Core.Enums.Relation.Friend, 1);
        arrangeContext.Persons.Add(person);
        await arrangeContext.SaveChangesAsync();
        arrangeContext.Occasions.Add(new Occasion(Core.Enums.OccasionType.Birthday, null, new DateOnly(1990, 5, 24), true, person.Id));
        await arrangeContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Should.ThrowAsync<ConflictException>(async () =>
            await _personService.UpdatePersonAsync(person.Id, 1, new PersonUpdateDto
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = null,
                Relation = Core.Enums.Relation.Friend
            }));

        exception.Message.ShouldBe("Das Geburtsdatum kann nicht entfernt werden, solange ein Geburtstags-Anlass besteht.");
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldDeleteOccasionsAndGifts()
    {
        // Arrange
        await using var arrangeContext = _dbContextFactory.CreateDbContext();
        var person = new Person("John", "Doe", new DateOnly(1990, 5, 24), Core.Enums.Relation.Friend, 1);
        arrangeContext.Persons.Add(person);
        await arrangeContext.SaveChangesAsync();
        var occasion = new Occasion(Core.Enums.OccasionType.Birthday, null, new DateOnly(1990, 5, 24), true, person.Id);
        arrangeContext.Occasions.Add(occasion);
        await arrangeContext.SaveChangesAsync();
        arrangeContext.Gifts.Add(new Gift("Kaffeemaschine", null, null, null, person.Id, occasion.Id));
        await arrangeContext.SaveChangesAsync();

        // Act
        await _personService.DeletePersonAsync(person.Id, 1);

        // Assert
        await using var assertContext = _dbContextFactory.CreateDbContext();
        (await assertContext.Persons.CountAsync()).ShouldBe(0);
        (await assertContext.Occasions.CountAsync()).ShouldBe(0);
        (await assertContext.Gifts.CountAsync()).ShouldBe(0);
    }
}