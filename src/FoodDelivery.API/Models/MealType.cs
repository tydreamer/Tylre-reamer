namespace FoodDelivery.API.Models;

public class MealType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Meal> Meals { get; set; } = [];
}
