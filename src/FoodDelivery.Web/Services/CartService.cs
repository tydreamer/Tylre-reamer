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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly Dictionary<int, CartItem> items = new();
    private DotNetObjectReference<CartService>? selfRef;
    private bool listenerRegistered;

    public int? RestaurantId { get; private set; }
    public string? RestaurantName { get; private set; }

    public IReadOnlyCollection<CartItem> Items => items.Values;
    public int TotalQuantity => items.Values.Sum(i => i.Quantity);
    public decimal Subtotal => items.Values.Sum(i => i.LineTotal);
    public bool IsEmpty => items.Count == 0;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        await LoadAsync();

        if (listenerRegistered)
            return;

        listenerRegistered = true;
        selfRef = DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("cartSync.register", selfRef);
    }

    public async Task RefreshFromStorageAsync()
    {
        await LoadAsync();
        OnChange?.Invoke();
    }

    public async Task AddAsync(MealDto meal, string restaurantName, int quantity = 1)
    {
        if (quantity <= 0)
            return;

        if (RestaurantId is not null && RestaurantId != meal.RestaurantId)
            ClearItems();

        RestaurantId = meal.RestaurantId;
        RestaurantName = restaurantName;

        if (items.TryGetValue(meal.Id, out var existing))
            existing.Quantity += quantity;
        else
            items[meal.Id] = new CartItem(meal, quantity);

        await NotifyAndPersistAsync();
    }

    public async Task SetQuantityAsync(int mealId, int quantity)
    {
        if (!items.TryGetValue(mealId, out var item))
            return;

        if (quantity <= 0)
            items.Remove(mealId);
        else
            item.Quantity = quantity;

        if (items.Count == 0)
            ClearRestaurant();

        await NotifyAndPersistAsync();
    }

    public async Task RemoveAsync(int mealId)
    {
        items.Remove(mealId);

        if (items.Count == 0)
            ClearRestaurant();

        await NotifyAndPersistAsync();
    }

    public async Task ClearAsync()
    {
        ClearItems();
        await NotifyAndPersistAsync();
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
        ClearRestaurant();
    }

    private void ClearRestaurant()
    {
        RestaurantId = null;
        RestaurantName = null;
    }

    private async Task NotifyAndPersistAsync()
    {
        await SaveAsync();
        OnChange?.Invoke();
    }

    private async Task LoadAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

            items.Clear();
            ClearRestaurant();

            if (string.IsNullOrWhiteSpace(json))
                return;

            var stored = JsonSerializer.Deserialize<StoredCart>(json, JsonOptions);
            if (stored?.Items is null || stored.Items.Count == 0)
                return;

            RestaurantId = stored.RestaurantId;
            RestaurantName = stored.RestaurantName;

            foreach (var item in stored.Items)
            {
                if (item.Meal is null || item.Quantity <= 0)
                    continue;

                items[item.Meal.Id] = new CartItem(item.Meal, item.Quantity);
            }
        }
        catch (JsonException)
        {
            items.Clear();
            ClearRestaurant();
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }

    private async Task SaveAsync()
    {
        if (items.Count == 0)
        {
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            return;
        }

        var stored = new StoredCart(
            RestaurantId,
            RestaurantName,
            items.Values.Select(i => new StoredItem(i.Meal, i.Quantity)).ToList());

        await js.InvokeVoidAsync(
            "localStorage.setItem",
            StorageKey,
            JsonSerializer.Serialize(stored, JsonOptions));
    }

    public ValueTask DisposeAsync()
    {
        selfRef?.Dispose();
        return ValueTask.CompletedTask;
    }

    private record StoredItem(MealDto Meal, int Quantity);
    private record StoredCart(int? RestaurantId, string? RestaurantName, List<StoredItem> Items);
}
