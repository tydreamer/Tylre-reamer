using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateRestaurantRequest(
    [Required(ErrorMessage = "Name is required."),
     MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")] string Name,
    [Required(ErrorMessage = "Description is required."),
     MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")] string Description,
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")] string? ImageUrl
);

public record UpdateRestaurantRequest(
    [Required(ErrorMessage = "Name is required."),
     MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")] string Name,
    [Required(ErrorMessage = "Description is required."),
     MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")] string Description,
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")] string? ImageUrl
);

public record RestaurantResponse(int Id, string Name, string Description, string ImageUrl, int OwnerId);
