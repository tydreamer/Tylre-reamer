namespace FoodDelivery.Web.Models.Users;

public record UserDto(int Id, string Name, string Email, string Role, bool IsBlocked);

public record CreateUserRequest(string Name, string Email, string Password, string Role);
