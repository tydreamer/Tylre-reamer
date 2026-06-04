using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs.Users;

public record CreateUserRequest(
    [Required(ErrorMessage = NameRequired), MaxLength(100, ErrorMessage = NameTooLong)] string Name,
    [Required(ErrorMessage = EmailRequired), ValidEmail(ErrorMessage = EmailInvalid), MaxLength(255, ErrorMessage = EmailTooLong)] string Email,
    [Required(ErrorMessage = PasswordRequired), MinLength(6, ErrorMessage = PasswordTooShort), MaxLength(100, ErrorMessage = PasswordTooLong)] string Password,
    [Required(ErrorMessage = RoleRequired)] string Role
);

public record UserResponse(int Id, string Name, string Email, string Role, bool IsBlocked);
