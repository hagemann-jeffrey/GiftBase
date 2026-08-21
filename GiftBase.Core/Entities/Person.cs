using GiftBase.Core.Enums;

namespace GiftBase.Core.Entities;

public class Person
{
    private Person() { }

    public Person(string firstName, string lastName, DateOnly? dateOfBirth, Relation relation, int userId)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Relation = relation;
        UserId = userId;
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly? DateOfBirth { get; private set; }
    public Relation Relation { get; private set; }
    public int UserId { get; private set; }
}
