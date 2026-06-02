using System.ComponentModel.DataAnnotations;
using FoodDelivery.Web.Constants;

namespace FoodDelivery.Web.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PasswordStrengthAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || string.IsNullOrWhiteSpace(password))
            return ValidationResult.Success;

        var result = PasswordStrengthResult.Evaluate(password);
        if (result.IsAcceptable)
            return ValidationResult.Success;

        var detail = result.UnmetRequirements.Count > 0
            ? string.Join(", ", result.UnmetRequirements)
            : "requirements not met";

        return new ValidationResult($"{ValidationMessages.PasswordTooWeak} ({detail}).");
    }
}
