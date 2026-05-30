using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateMealRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [MaxLength(500)] string? ImageUrl,
    [Range(0.01, 100_000)] decimal Price
);

public record UpdateMealRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [MaxLength(500)] string? ImageUrl,
    [Range(0.01, 100_000)] decimal Price,
    bool IsAvailable
);

public record MealResponse(int Id, string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable, int RestaurantId);
