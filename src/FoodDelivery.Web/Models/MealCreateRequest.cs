namespace FoodDelivery.Web.Models;

public record MealCreateRequest(
    string Name,
    string Description,
    string ImageUrl,
    decimal Price
);
