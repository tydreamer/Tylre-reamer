using System.ComponentModel.DataAnnotations;
using FoodDelivery.Web.Constants;

namespace FoodDelivery.Web.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NotBeforeTodayAttribute : ValidationAttribute
{
    public NotBeforeTodayAttribute() => ErrorMessage = ValidationMessages.ExpiresAtNotBeforeToday;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime date)
            return ValidationResult.Success;

        return date.Date < DateTime.Today
            ? new ValidationResult(ErrorMessage)
            : ValidationResult.Success;
    }
}
