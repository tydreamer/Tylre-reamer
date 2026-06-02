namespace FoodDelivery.API.Constants;

public static class CuisineNames
{
    public const string Italian = "Italian";
    public const string French = "French";
    public const string Chinese = "Chinese";
    public const string Japanese = "Japanese";
    public const string Mexican = "Mexican";

    public static readonly IReadOnlyList<string> All =
    [
        Italian,
        French,
        Chinese,
        Japanese,
        Mexican
    ];
}
