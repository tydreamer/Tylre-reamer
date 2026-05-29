using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class CartItem(MealDto meal, int quantity)
{
    public MealDto Meal { get; } = meal;
    public int Quantity { get; set; } = quantity;
    public decimal LineTotal => Meal.Price * Quantity;
}

public class CartService
{
    private readonly Dictionary<int, CartItem> items = new();

    public int? RestaurantId { get; private set; }
    public string? RestaurantName { get; private set; }

    public IReadOnlyCollection<CartItem> Items => items.Values;
    public int TotalQuantity => items.Values.Sum(i => i.Quantity);
    public decimal Subtotal => items.Values.Sum(i => i.LineTotal);
    public bool IsEmpty => items.Count == 0;

    public event Action? OnChange;

    public void Add(MealDto meal, string restaurantName)
    {
        if (RestaurantId is not null && RestaurantId != meal.RestaurantId)
            ClearItems();

        RestaurantId = meal.RestaurantId;
        RestaurantName = restaurantName;

        if (items.TryGetValue(meal.Id, out var existing))
            existing.Quantity++;
        else
            items[meal.Id] = new CartItem(meal, 1);

        OnChange?.Invoke();
    }

    public void SetQuantity(int mealId, int quantity)
    {
        if (!items.TryGetValue(mealId, out var item))
            return;

        if (quantity <= 0)
            items.Remove(mealId);
        else
            item.Quantity = quantity;

        OnChange?.Invoke();
    }

    public void Remove(int mealId)
    {
        items.Remove(mealId);
        OnChange?.Invoke();
    }

    public void Clear()
    {
        ClearItems();
        OnChange?.Invoke();
    }

    private void ClearItems()
    {
        items.Clear();
        RestaurantId = null;
        RestaurantName = null;
    }
}
