using System.ComponentModel.DataAnnotations;

namespace GiftBase.Options;

public class EmailOptions
{
    public const string SectionName = "EmailSettings";

    [Required(ErrorMessage = "EmailSettings:SmtpServer ist nicht konfiguriert.")]
    public string SmtpServer { get; set; } = null!;

    [Range(1, 65535, ErrorMessage = "EmailSettings:SmtpPort ist keine gültige Portnummer.")]
    public int SmtpPort { get; set; }

    [Required(ErrorMessage = "EmailSettings:SmtpUsername ist nicht konfiguriert.")]
    public string SmtpUsername { get; set; } = null!;

    [Required(ErrorMessage = "EmailSettings:SmtpPassword ist nicht konfiguriert.")]
    public string SmtpPassword { get; set; } = null!;

    [Required(ErrorMessage = "EmailSettings:SenderEmail ist nicht konfiguriert.")]
    public string SenderEmail { get; set; } = null!;

    public string? SenderName { get; set; }
}
