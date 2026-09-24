using System.ComponentModel.DataAnnotations;

namespace GiftBase.Features.GiftSuggestions;

public class GiftSuggestionInput : IValidatableObject
{
    [Required(ErrorMessage = "Anlass ist erforderlich.")]
    public int? OccasionId { get; set; }
    [Required(ErrorMessage = "Budget ist erforderlich.")]
    [Range(1, 99_999_999, ErrorMessage = "Budget muss zwischen 1 und 99.999.999 liegen.")]
    public decimal? Budget { get; set; }
    [MaxLength(500, ErrorMessage = "Hinweis darf maximal 500 Zeichen lang sein.")]
    public string? AdditionalHint { get; set; }
    public bool ConsentGiven { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ConsentGiven)
        {
            yield return new ValidationResult("Bitte bestätige den Hinweis zur Datenübertragung.", [nameof(ConsentGiven)]);
        }
    }
}
