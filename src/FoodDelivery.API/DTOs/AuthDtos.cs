namespace FoodDelivery.API.DTOs;

public record RegisterRequest(string Name, string Email, string Password, string Role);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Name, string Email, string Role);
