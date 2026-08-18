using System.ComponentModel.DataAnnotations;

namespace GiftBase.Tests.Helper;

public static class ValidationHelper
{
    public static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}
