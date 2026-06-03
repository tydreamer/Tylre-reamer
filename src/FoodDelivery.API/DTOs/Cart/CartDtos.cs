using System.ComponentModel.DataAnnotations;
using FoodDelivery.API.DTOs.Meals;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs.Cart;

public record AddCartItemRequest(
    [Range(1, int.MaxValue)] int MealId,
    [Range(1, 100, ErrorMessage = QuantityInvalid)] int Quantity);

public record SetCartItemQuantityRequest(
    [Range(0, 100, ErrorMessage = QuantityInvalid)] int Quantity);

public record CartItemResponse(MealResponse Meal, int Quantity);

public record CartResponse(
    int? RestaurantId,
    string? RestaurantName,
    IReadOnlyList<CartItemResponse> Items);
