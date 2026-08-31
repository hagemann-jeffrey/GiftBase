using System.ComponentModel.DataAnnotations;

namespace GiftBase.Shared.Validation;

public class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute()
    {
        ErrorMessage = "Bitte gib einen gültigen Link an, der mit http:// oder https:// beginnt.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string link || string.IsNullOrWhiteSpace(link))
        {
            return ValidationResult.Success;
        }

        if (Uri.TryCreate(link, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            "Bitte gib einen gültigen Link an, der mit http:// oder https:// beginnt.",
            validationContext.MemberName is not null ? [validationContext.MemberName] : Array.Empty<string>());
    }
}