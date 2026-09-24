using System.ComponentModel.DataAnnotations;

namespace GiftBase.Options;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    [Required(ErrorMessage = "Gemini:ApiKey ist nicht konfiguriert.")]
    public string ApiKey { get; set; } = null!;

    [Required(ErrorMessage = "Gemini:Model ist nicht konfiguriert.")]
    public string Model { get; set; } = null!;
}
