using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs.Auth;

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

public record ForgotPasswordRequest(
    [Required(ErrorMessage = EmailRequired), EmailAddress(ErrorMessage = EmailInvalid), MaxLength(255, ErrorMessage = EmailTooLong)] string Email
);

public record ResetPasswordRequest(
    [Required] string Token,
    [Required(ErrorMessage = PasswordRequired), MinLength(6, ErrorMessage = PasswordTooShort), MaxLength(100, ErrorMessage = PasswordTooLong)] string NewPassword
);

public record ForgotPasswordResponse(string Message, string? ResetUrl = null);
