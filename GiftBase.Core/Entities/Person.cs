using GiftBase.Core.Dtos;
using GiftBase.Core.Enums;

namespace GiftBase.Core.Entities;

public class Person
{
    private Person() { }

    public Person(string firstName, string lastName, DateOnly? dateOfBirth, Relation relation, int userId, string? interests = null)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Relation = relation;
        UserId = userId;
        Interests = interests;
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly? DateOfBirth { get; private set; }
    public Relation Relation { get; private set; }
    public int UserId { get; private set; }
    public string? Interests { get; private set; }

    public List<Gift> Gifts { get; private set; } = [];
    public List<Occasion> Occasions { get; private set; } = [];

    public void Update(PersonUpdateDto personUpdateDto)
    {
        FirstName = personUpdateDto.FirstName;
        LastName = personUpdateDto.LastName;
        DateOfBirth = personUpdateDto.DateOfBirth;
        Relation = personUpdateDto.Relation;
        Interests = personUpdateDto.Interests;
    }

    public int? GetAge(DateOnly today)
    {
        if (!DateOfBirth.HasValue)
        {
            return null;
        }

        var age = today.Year - DateOfBirth.Value.Year;

        if (DateOfBirth.Value > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
