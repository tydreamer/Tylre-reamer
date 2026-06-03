namespace FoodDelivery.Web.Models.Meals;

public record MealUpdateRequest(
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    int MealTypeId
);
