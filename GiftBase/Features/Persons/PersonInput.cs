using System.ComponentModel.DataAnnotations;

namespace GiftBase.Features.Persons
{
    public class PersonInput
    {
        [Required(ErrorMessage = "Vorname ist erforderlich.")]
        [MaxLength(100, ErrorMessage = "Vorname darf maximal 100 Zeichen lang sein.")]
        public string FirstName { get; set; } = null!;
        [Required(ErrorMessage = "Nachname ist erforderlich.")]
        [MaxLength(100, ErrorMessage = "Nachname darf maximal 100 Zeichen lang sein.")]
        public string LastName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public Core.Enums.Relation Relation { get; set; }
        [MaxLength(200, ErrorMessage = "Interessen dürfen maximal 200 Zeichen lang sein.")]
        public string? Interests { get; set; }
    }
}