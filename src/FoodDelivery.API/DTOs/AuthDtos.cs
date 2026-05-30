using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record RegisterRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required] string Role
);

public record LoginRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required] string Password
);

public record LoginResponse(string Token, string Name, string Email, string Role);
