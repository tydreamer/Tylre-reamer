using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs;

public record CreateMealRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [MaxLength(500, ErrorMessage = DescriptionTooLong)] string? Description,
    [MaxLength(500, ErrorMessage = ImageUrlTooLong)] string? ImageUrl,
    [Range(0.01, 100_000, ErrorMessage = PriceInvalid)] decimal Price
);

public record UpdateMealRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [MaxLength(500, ErrorMessage = DescriptionTooLong)] string? Description,
    [MaxLength(500, ErrorMessage = ImageUrlTooLong)] string? ImageUrl,
    [Range(0.01, 100_000, ErrorMessage = PriceInvalid)] decimal Price,
    bool IsAvailable
);

public record MealResponse(int Id, string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable, int RestaurantId);

public record MealBrowseResponse(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    bool IsAvailable,
    int RestaurantId,
    string RestaurantName);
