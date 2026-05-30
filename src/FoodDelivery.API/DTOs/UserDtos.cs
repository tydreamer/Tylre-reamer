namespace FoodDelivery.API.DTOs;

public record CreateUserRequest(string Name, string Email, string Password, string Role);

public record UserResponse(int Id, string Name, string Email, string Role, bool IsBlocked);
