namespace FoodDelivery.Web.Models.Meals;

public record MealBrowseDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    bool IsAvailable,
    int RestaurantId,
    string RestaurantName,
    int MealTypeId,
    string MealTypeName
);
