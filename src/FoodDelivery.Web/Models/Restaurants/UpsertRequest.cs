namespace FoodDelivery.Web.Models.Restaurants;

public record RestaurantUpsertRequest(
    string Name,
    string Description,
    string ImageUrl,
    int CuisineId,
    double Latitude,
    double Longitude
);
