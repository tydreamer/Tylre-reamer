using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs;

public record RegisterRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [Required(ErrorMessage = EmailRequired), EmailAddress(ErrorMessage = EmailInvalid), MaxLength(255, ErrorMessage = EmailTooLong)] string Email,
    [Required(ErrorMessage = PasswordRequired), MinLength(6, ErrorMessage = PasswordTooShort), MaxLength(100, ErrorMessage = PasswordTooLong)] string Password,
    [Required(ErrorMessage = RoleRequired)] string Role
);

public record LoginRequest(
    [Required(ErrorMessage = EmailRequired), EmailAddress(ErrorMessage = EmailInvalid), MaxLength(255, ErrorMessage = EmailTooLong)] string Email,
    [Required(ErrorMessage = PasswordRequired)] string Password
);

public record LoginResponse(string Token, string Name, string Email, string Role);
