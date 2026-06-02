using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    public int CuisineId { get; set; }
    public Cuisine Cuisine { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<Meal> Meals { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
