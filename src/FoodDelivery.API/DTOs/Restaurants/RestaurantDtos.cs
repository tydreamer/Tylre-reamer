using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs.Restaurants;

public record CreateRestaurantRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [Required(ErrorMessage = DescriptionRequired), MaxLength(500, ErrorMessage = DescriptionTooLong)] string Description,
    [MaxLength(500, ErrorMessage = ImageUrlTooLong)] string? ImageUrl,
    [Range(1, int.MaxValue, ErrorMessage = CuisineRequired)] int CuisineId,
    [Range(-90, 90, ErrorMessage = LatitudeInvalid)] double Latitude,
    [Range(-180, 180, ErrorMessage = LongitudeInvalid)] double Longitude
);

public record UpdateRestaurantRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [Required(ErrorMessage = DescriptionRequired), MaxLength(500, ErrorMessage = DescriptionTooLong)] string Description,
    [MaxLength(500, ErrorMessage = ImageUrlTooLong)] string? ImageUrl,
    [Range(1, int.MaxValue, ErrorMessage = CuisineRequired)] int CuisineId,
    [Range(-90, 90, ErrorMessage = LatitudeInvalid)] double Latitude,
    [Range(-180, 180, ErrorMessage = LongitudeInvalid)] double Longitude
);

public record RestaurantResponse(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    int OwnerId,
    string OwnerEmail,
    int CuisineId,
    string CuisineName,
    double Latitude,
    double Longitude);
