namespace FoodDelivery.Web.Models;

public record MealUpdateRequest(
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    bool IsAvailable
);
