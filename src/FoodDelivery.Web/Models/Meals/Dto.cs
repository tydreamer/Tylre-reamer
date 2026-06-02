namespace FoodDelivery.Web.Models.Meals;

public record MealDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    bool IsAvailable,
    int RestaurantId,
    int MealTypeId,
    string MealTypeName
);
