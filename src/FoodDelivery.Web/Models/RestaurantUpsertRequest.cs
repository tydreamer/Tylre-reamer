namespace FoodDelivery.Web.Models;

public record RestaurantUpsertRequest(
    string Name,
    string Description,
    string ImageUrl
);
