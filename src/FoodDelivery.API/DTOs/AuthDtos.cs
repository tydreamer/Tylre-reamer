using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record RegisterRequest(
    [Required(ErrorMessage = "Name is required."),
     MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")] string Name,
    [Required(ErrorMessage = "Email is required."),
     EmailAddress(ErrorMessage = "Enter a valid email address."),
     MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")] string Email,
    [Required(ErrorMessage = "Password is required."),
     MinLength(6, ErrorMessage = "Password must be at least 6 characters."),
     MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")] string Password,
    [Required(ErrorMessage = "Role is required.")] string Role
);

public record LoginRequest(
    [Required(ErrorMessage = "Email is required."),
     EmailAddress(ErrorMessage = "Enter a valid email address."),
     MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")] string Email,
    [Required(ErrorMessage = "Password is required.")] string Password
);

public record LoginResponse(string Token, string Name, string Email, string Role);
