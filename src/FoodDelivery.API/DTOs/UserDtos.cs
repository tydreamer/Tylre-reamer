using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateUserRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required, EmailAddress, MaxLength(255)] string Email,
    [property: Required, MinLength(6), MaxLength(100)] string Password,
    [property: Required] string Role
);

public record UserResponse(int Id, string Name, string Email, string Role, bool IsBlocked);
