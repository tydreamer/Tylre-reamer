namespace FoodDelivery.Web.Helpers;

public static class DisplayTextHelper
{
    public const int CouponCodeDisplayLength = 16;

    public static string TruncateWithEllipsis(string? value, int maxLength = CouponCodeDisplayLength)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? "";

        if (value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }
}
