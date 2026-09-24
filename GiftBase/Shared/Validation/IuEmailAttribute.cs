using System.ComponentModel.DataAnnotations;
using GiftBase.Shared.Common;

namespace GiftBase.Shared.Validation;

public class IuEmailAttribute : ValidationAttribute
{
    public IuEmailAttribute()
    {
        ErrorMessage = "Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string email && RegistrationDomains.IsAllowed(email))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            "Bitte registriere dich mit deiner offiziellen Hochschul-E-Mail-Adresse.",
            validationContext.MemberName is not null ? [validationContext.MemberName] : Array.Empty<string>());
    }
}
