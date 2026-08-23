namespace GiftBase.Core.Dtos;

public class PersonAddDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public Enums.Relation Relation { get; set; }
    public int UserId { get; set; }
}