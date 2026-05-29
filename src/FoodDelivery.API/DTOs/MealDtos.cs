namespace FoodDelivery.API.DTOs;

public record CreateMealRequest(string Name, string Description, string ImageUrl, decimal Price);
public record UpdateMealRequest(string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable);

public record MealResponse(int Id, string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable, int RestaurantId);
