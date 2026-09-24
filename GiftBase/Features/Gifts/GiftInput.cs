using System.ComponentModel.DataAnnotations;
using GiftBase.Shared.Validation;

namespace GiftBase.Features.Gifts;

public class GiftInput
{
    [Required(ErrorMessage = "Titel ist erforderlich.")]
    [MaxLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen lang sein.")]
    public string Title { get; set; } = null!;
    [MaxLength(1000, ErrorMessage = "Notiz darf maximal 1000 Zeichen lang sein.")]
    public string? Note { get; set; }
    [MaxLength(2000, ErrorMessage = "Link darf maximal 2000 Zeichen lang sein.")]
    [HttpUrl]
    public string? Link { get; set; }
    [Range(0, 99_999_999, ErrorMessage = "Preis muss zwischen 0 und 99.999.999 liegen.")]
    public decimal? Price { get; set; }
    public Core.Enums.GiftStatus Status { get; set; }
    public int? OccasionId { get; set; }
    public string? OccasionLabel { get; set; }
    public int? OccasionYear { get; set; }
    public Guid? ImageVersion { get; set; }
    public byte[]? ImageContent { get; set; }
    public string? ImageContentType { get; set; }
}
