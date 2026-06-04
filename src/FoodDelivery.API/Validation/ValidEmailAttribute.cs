using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FoodDelivery.API.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
public sealed class ValidEmailAttribute : ValidationAttribute
{
    // Requires: localpart@domain.tld  (TLD must be at least 2 letters)
    private static readonly Regex Pattern = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        return value is string s && Pattern.IsMatch(s.Trim());
    }
}
