using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record RegisterRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required, EmailAddress, MaxLength(255)] string Email,
    [property: Required, MinLength(6), MaxLength(100)] string Password,
    [property: Required] string Role
);

public record LoginRequest(
    [property: Required, EmailAddress, MaxLength(255)] string Email,
    [property: Required] string Password
);

public record LoginResponse(string Token, string Name, string Email, string Role);
