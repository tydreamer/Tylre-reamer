namespace FoodDelivery.API.DTOs;

public record CreateRestaurantRequest(string Name, string Description, string ImageUrl);
public record UpdateRestaurantRequest(string Name, string Description, string ImageUrl);

public record RestaurantResponse(int Id, string Name, string Description, string ImageUrl, int OwnerId);
