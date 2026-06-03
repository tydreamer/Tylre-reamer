namespace FoodDelivery.Web.Models.Cart;

public record CartItemDto(MealDto Meal, int Quantity);

public record CartDto(
    int? RestaurantId,
    string? RestaurantName,
    IReadOnlyList<CartItemDto> Items);

public record AddCartItemRequest(int MealId, int Quantity);

public record SetCartItemQuantityRequest(int Quantity);
