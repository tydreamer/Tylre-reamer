using FoodDelivery.API.DTOs;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.Helpers;

public static class RestaurantMapper
{
    public static RestaurantResponse ToResponse(Restaurant restaurant) =>
        new(
            restaurant.Id,
            restaurant.Name,
            restaurant.Description,
            restaurant.ImageUrl,
            restaurant.OwnerId,
            restaurant.CuisineId,
            restaurant.Cuisine.Name,
            restaurant.Latitude,
            restaurant.Longitude);
}
