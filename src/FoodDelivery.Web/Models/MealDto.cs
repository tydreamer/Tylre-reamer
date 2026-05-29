namespace FoodDelivery.Web.Models;

public record MealDto(int Id, string Name, string Description, string ImageUrl, decimal Price, bool IsAvailable, int RestaurantId);
