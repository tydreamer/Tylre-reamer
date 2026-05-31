using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Helpers;

public static class OrderItemsFormatter
{
    public const int MaxDisplayLength = 80;

    public static string FormatFull(IEnumerable<OrderItemResponse> items) =>
        string.Join(", ", items.Select(FormatLine));

    public static (string Display, string Full) FormatForDisplay(IEnumerable<OrderItemResponse> items)
    {
        var full = FormatFull(items);
        if (string.IsNullOrEmpty(full))
            return ("—", "");

        if (full.Length <= MaxDisplayLength)
            return (full, full);

        return (full[..(MaxDisplayLength - 3)] + "...", full);
    }

    private static string FormatLine(OrderItemResponse item) =>
        item.Quantity > 1 ? $"{item.MealName} × {item.Quantity}" : item.MealName;
}
