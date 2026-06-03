using FoodDelivery.API.Models;

namespace FoodDelivery.API.Helpers.Restaurants;

public static class RestaurantMapper
{
    public static RestaurantResponse ToResponse(Restaurant restaurant) =>
        new(
            restaurant.Id,
            restaurant.Name,
            restaurant.Description,
            restaurant.ImageUrl,
            restaurant.OwnerId,
            restaurant.Owner.Email,
            restaurant.CuisineId,
            restaurant.Cuisine.Name,
            restaurant.Latitude,
            restaurant.Longitude);
}
