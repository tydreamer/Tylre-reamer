namespace FoodDelivery.API.Models;

public class Meal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
