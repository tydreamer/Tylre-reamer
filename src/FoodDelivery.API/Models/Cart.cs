namespace FoodDelivery.API.Models;

public class Cart
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public int? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public ICollection<CartItem> Items { get; set; } = [];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    public int MealId { get; set; }
    public Meal Meal { get; set; } = null!;
    public int Quantity { get; set; }
}
