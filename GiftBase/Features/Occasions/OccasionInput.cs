using System.ComponentModel.DataAnnotations;

namespace GiftBase.Features.Occasions
{
    public class OccasionInput : IValidatableObject
    {
        public Core.Enums.OccasionType Type { get; set; } = Core.Enums.OccasionType.Custom;
        [MaxLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen lang sein.")]
        public string? Title { get; set; }
        public DateTime? Date { get; set; }
        public bool IsRecurring { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type != Core.Enums.OccasionType.Custom)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                yield return new ValidationResult("Titel ist erforderlich.", [nameof(Title)]);
            }

            if (!Date.HasValue)
            {
                yield return new ValidationResult("Datum ist erforderlich.", [nameof(Date)]);
            }
        }
    }
}
