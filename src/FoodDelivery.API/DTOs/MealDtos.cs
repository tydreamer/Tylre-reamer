using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateMealRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: MaxLength(500)] string? Description,
    [property: MaxLength(500)] string? ImageUrl,
    [property: Range(0.01, 100_000)] decimal Price
);

public record UpdateMealRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: MaxLength(500)] string? Description,
    [property: MaxLength(500)] string? ImageUrl,
    [property: Range(0.01, 100_000)] decimal Price,
    bool IsAvailable
);

public record MealResponse(int Id, string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable, int RestaurantId);
