using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateMealRequest(
    [Required(ErrorMessage = "Name is required."),
     MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")] string Name,
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")] string? Description,
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")] string? ImageUrl,
    [Range(0.01, 100_000, ErrorMessage = "Price must be greater than 0.")] decimal Price
);

public record UpdateMealRequest(
    [Required(ErrorMessage = "Name is required."),
     MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")] string Name,
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")] string? Description,
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")] string? ImageUrl,
    [Range(0.01, 100_000, ErrorMessage = "Price must be greater than 0.")] decimal Price,
    bool IsAvailable
);

public record MealResponse(int Id, string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable, int RestaurantId);
