namespace FoodDelivery.API.Models;

public class Cuisine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Restaurant> Restaurants { get; set; } = [];
}
