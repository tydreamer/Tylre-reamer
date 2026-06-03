namespace FoodDelivery.Web.Models.Restaurants;

public record RestaurantDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    int OwnerId,
    string OwnerEmail,
    int CuisineId,
    string CuisineName,
    double Latitude,
    double Longitude
);
