using System.Text.Json;
using FoodDelivery.Web.Models;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public class CartItem(MealDto meal, int quantity)
{
    public MealDto Meal { get; } = meal;
    public int Quantity { get; set; } = quantity;
    public decimal LineTotal => Meal.Price * Quantity;
}

public class CartService(IJSRuntime js) : IAsyncDisposable
{
    private const string StorageKey = "cart";

    private readonly Dictionary<int, CartItem> items = new();
    private DotNetObjectReference<CartService>? selfRef;
    private bool initialized;

    public int? RestaurantId { get; private set; }
    public string? RestaurantName { get; private set; }

    public IReadOnlyCollection<CartItem> Items => items.Values;
    public int TotalQuantity => items.Values.Sum(i => i.Quantity);
    public decimal Subtotal => items.Values.Sum(i => i.LineTotal);
    public bool IsEmpty => items.Count == 0;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (initialized)
            return;
        initialized = true;

        await LoadAsync();
        selfRef = DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("cartSync.register", selfRef);
    }

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

        NotifyAndPersist();
    }

    public void SetQuantity(int mealId, int quantity)
    {
        if (!items.TryGetValue(mealId, out var item))
            return;

        if (quantity <= 0)
            items.Remove(mealId);
        else
            item.Quantity = quantity;

        NotifyAndPersist();
    }

    public void Remove(int mealId)
    {
        items.Remove(mealId);
        NotifyAndPersist();
    }

    public void Clear()
    {
        ClearItems();
        NotifyAndPersist();
    }

    [JSInvokable]
    public async Task OnCartChangedExternally()
    {
        await LoadAsync();
        OnChange?.Invoke();
    }

    private void ClearItems()
    {
        items.Clear();
        RestaurantId = null;
        RestaurantName = null;
    }

    private void NotifyAndPersist()
    {
        OnChange?.Invoke();
        _ = SaveAsync();
    }

    private async Task LoadAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

        ClearItems();

        if (string.IsNullOrWhiteSpace(json))
            return;

        var stored = JsonSerializer.Deserialize<StoredCart>(json);
        if (stored is null)
            return;

        RestaurantId = stored.RestaurantId;
        RestaurantName = stored.RestaurantName;
        foreach (var item in stored.Items)
            items[item.Meal.Id] = new CartItem(item.Meal, item.Quantity);
    }

    private async Task SaveAsync()
    {
        var stored = new StoredCart(
            RestaurantId,
            RestaurantName,
            items.Values.Select(i => new StoredItem(i.Meal, i.Quantity)).ToList());

        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(stored));
    }

    public ValueTask DisposeAsync()
    {
        selfRef?.Dispose();
        return ValueTask.CompletedTask;
    }

    private record StoredItem(MealDto Meal, int Quantity);
    private record StoredCart(int? RestaurantId, string? RestaurantName, List<StoredItem> Items);
}
