namespace FoodDelivery.API.Constants;

public static class MealTypeNames
{
    public const string Breakfast = "Breakfast";
    public const string Lunch = "Lunch";
    public const string Dinner = "Dinner";
    public const string Appetizers = "Appetizers";
    public const string Dessert = "Dessert";

    public static readonly IReadOnlyList<string> All =
    [
        Breakfast,
        Lunch,
        Dinner,
        Appetizers,
        Dessert
    ];
}
