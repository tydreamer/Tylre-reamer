namespace FoodDelivery.Web.Models.Restaurants;

public record RestaurantDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    int OwnerId,
    int CuisineId,
    string CuisineName,
    double Latitude,
    double Longitude
);
