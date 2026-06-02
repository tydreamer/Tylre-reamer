namespace FoodDelivery.Web.Models.Meals;

public record MealCreateRequest(
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    int MealTypeId
);
