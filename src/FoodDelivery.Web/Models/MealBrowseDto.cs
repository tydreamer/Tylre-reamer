namespace FoodDelivery.Web.Models;

public record MealBrowseDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    bool IsAvailable,
    int RestaurantId,
    string RestaurantName
);
