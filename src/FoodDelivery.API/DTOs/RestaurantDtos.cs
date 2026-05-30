using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateRestaurantRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(500)] string Description,
    [MaxLength(500)] string? ImageUrl
);

public record UpdateRestaurantRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(500)] string Description,
    [MaxLength(500)] string? ImageUrl
);

public record RestaurantResponse(int Id, string Name, string Description, string ImageUrl, int OwnerId);
