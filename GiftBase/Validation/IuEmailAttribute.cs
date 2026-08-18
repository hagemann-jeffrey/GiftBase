using System.ComponentModel.DataAnnotations;

namespace GiftBase.Validation;

public class IuEmailAttribute : ValidationAttribute
{
    private readonly string[] _allowedDomains = { "iu-study.org", "iu.org" };

    public IuEmailAttribute()
    {
        ErrorMessage = "Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string email)
        {
            if (_allowedDomains.Any(domain => email.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase)))
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(
            "Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.",
            validationContext.MemberName is not null ? [validationContext.MemberName] : Array.Empty<string>());
    }
}