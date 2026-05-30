using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateRestaurantRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required, MaxLength(500)] string Description,
    [property: MaxLength(500)] string? ImageUrl
);

public record UpdateRestaurantRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required, MaxLength(500)] string Description,
    [property: MaxLength(500)] string? ImageUrl
);

public record RestaurantResponse(int Id, string Name, string Description, string ImageUrl, int OwnerId);
