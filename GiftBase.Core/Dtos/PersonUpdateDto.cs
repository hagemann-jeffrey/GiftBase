namespace GiftBase.Core.Dtos;

public class PersonUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public Enums.Relation Relation { get; set; }
}