namespace FoodDelivery.Web.Models;

public record RestaurantDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    int OwnerId
);
