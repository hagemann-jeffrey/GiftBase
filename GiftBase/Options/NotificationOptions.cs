using System.ComponentModel.DataAnnotations;

namespace GiftBase.Options;

public class NotificationOptions
{
    public const string SectionName = "Notifications";

    [Required(ErrorMessage = "Notifications:TriggerSecret ist nicht konfiguriert.")]
    public string TriggerSecret { get; set; } = null!;
}
