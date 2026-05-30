using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateUserRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required] string Role
);

public record UserResponse(int Id, string Name, string Email, string Role, bool IsBlocked);
