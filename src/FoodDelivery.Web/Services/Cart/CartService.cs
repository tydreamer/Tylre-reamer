using System.Net.Http.Json;
using FoodDelivery.Web.Extensions;
using FoodDelivery.Web.Models.Cart;
using Microsoft.AspNetCore.Components.Authorization;

namespace FoodDelivery.Web.Services.Cart;

public class CartItem(MealDto meal, int quantity)
{
    public MealDto Meal { get; } = meal;
    public int Quantity { get; set; } = quantity;
    public decimal LineTotal => Meal.Price * Quantity;
}

public class CartService(HttpClient http, AuthenticationStateProvider authStateProvider) : IDisposable
{
    private readonly Dictionary<int, CartItem> items = new();
    private bool listenerRegistered;
    private int? currentUserId;

    public int? RestaurantId { get; private set; }
    public string? RestaurantName { get; private set; }

    public IReadOnlyCollection<CartItem> Items => items.Values;
    public int TotalQuantity => items.Values.Sum(i => i.Quantity);
    public decimal Subtotal => items.Values.Sum(i => i.LineTotal);
    public bool IsEmpty => items.Count == 0;

    public bool HasItemsFromDifferentRestaurant(int restaurantId) =>
        !IsEmpty && RestaurantId != restaurantId;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        await UpdateUserContextAsync();
        await LoadAsync();

        if (listenerRegistered)
            return;

        listenerRegistered = true;
        authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
        OnChange?.Invoke();
    }

    public async Task AddAsync(MealDto meal, string restaurantName, int quantity = 1)
    {
        if (quantity <= 0 || currentUserId is null)
            return;

        if (HasItemsFromDifferentRestaurant(meal.RestaurantId))
            return;

        var response = await http.PostAsJsonAsync(
            "api/cart/items",
            new AddCartItemRequest(meal.Id, quantity));

        await ApplyResponseAsync(response);
    }

    public async Task SetQuantityAsync(int mealId, int quantity)
    {
        if (currentUserId is null)
            return;

        var response = await http.PutAsJsonAsync(
            $"api/cart/items/{mealId}",
            new SetCartItemQuantityRequest(quantity));

        await ApplyResponseAsync(response);
    }

    public async Task RemoveAsync(int mealId)
    {
        if (currentUserId is null)
            return;

        var response = await http.DeleteAsync($"api/cart/items/{mealId}");
        await ApplyResponseAsync(response);
    }

    public async Task ClearAsync()
    {
        if (currentUserId is null)
        {
            ClearItems();
            OnChange?.Invoke();
            return;
        }

        var response = await http.DeleteAsync("api/cart");
        await ApplyResponseAsync(response);
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> authStateTask)
    {
        try
        {
            var authState = await authStateTask;
            if (!await UpdateUserContextAsync(authState))
                return;

            await LoadAsync();
            OnChange?.Invoke();
        }
        catch
        {
            // Ignore auth/cart sync failures during sign-in/out transitions.
        }
    }

    private async Task<bool> UpdateUserContextAsync(AuthenticationState? authState = null)
    {
        authState ??= await authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.GetUserId();

        if (userId == currentUserId)
            return false;

        currentUserId = userId;
        return true;
    }

    private async Task LoadAsync()
    {
        await UpdateUserContextAsync();
        ClearItems();

        if (currentUserId is null)
            return;

        try
        {
            var cart = await http.GetFromJsonAsync<CartDto>("api/cart");
            if (cart is not null)
                ApplyCart(cart);
        }
        catch
        {
            ClearItems();
        }
    }

    private async Task<bool> ApplyResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var cart = await response.Content.ReadFromJsonAsync<CartDto>();
            ApplyCart(cart ?? new CartDto(null, null, []));
            OnChange?.Invoke();
            return true;
        }

        await LoadAsync();
        OnChange?.Invoke();
        return false;
    }

    private void ApplyCart(CartDto cart)
    {
        ClearItems();
        RestaurantId = cart.RestaurantId;
        RestaurantName = cart.RestaurantName;

        foreach (var item in cart.Items)
        {
            if (item.Quantity <= 0)
                continue;

            items[item.Meal.Id] = new CartItem(item.Meal, item.Quantity);
        }
    }

    private void ClearItems()
    {
        items.Clear();
        RestaurantId = null;
        RestaurantName = null;
    }

    public void Dispose()
    {
        authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
