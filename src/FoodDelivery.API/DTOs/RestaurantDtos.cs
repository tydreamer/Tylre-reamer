using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs;

public record CreateRestaurantRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [Required(ErrorMessage = DescriptionRequired), MaxLength(500, ErrorMessage = DescriptionTooLong)] string Description,
    [MaxLength(500, ErrorMessage = ImageUrlTooLong)] string? ImageUrl
);

public record UpdateRestaurantRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [Required(ErrorMessage = DescriptionRequired), MaxLength(500, ErrorMessage = DescriptionTooLong)] string Description,
    [MaxLength(500, ErrorMessage = ImageUrlTooLong)] string? ImageUrl
);

public record RestaurantResponse(int Id, string Name, string Description, string ImageUrl, int OwnerId);
