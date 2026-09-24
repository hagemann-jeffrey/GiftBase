using GiftBase.Core.Enums;

namespace GiftBase.Core.Dtos;

public class PersonUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public Relation Relation { get; set; }
    public string? Interests { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
}