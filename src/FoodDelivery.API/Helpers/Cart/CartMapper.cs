using FoodDelivery.API.DTOs.Cart;
using FoodDelivery.API.Helpers.Meals;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.Helpers.Cart;

public static class CartMapper
{
    public static CartResponse ToResponse(Models.Cart cart) =>
        new(
            cart.RestaurantId,
            cart.Restaurant?.Name,
            cart.Items
                .OrderBy(i => i.Meal.MealType.Name)
                .ThenBy(i => i.Meal.Name)
                .Select(i => new CartItemResponse(MealMapper.ToResponse(i.Meal), i.Quantity))
                .ToList());

    public static CartResponse Empty() => new(null, null, []);
}
