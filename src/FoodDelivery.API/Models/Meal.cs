using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.Models;

public class Meal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MealTypeId { get; set; }
    public MealType MealType { get; set; } = null!;
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
