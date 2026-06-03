using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Helpers.Images;

public static class OwnerRestaurantAccess
{
    public static int GetOwnerId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static async Task<(Restaurant? Restaurant, IActionResult? Error)> RequireOwnedRestaurantAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        int restaurantId)
    {
        var ownerId = GetOwnerId(user);
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null)
            return (null, new NotFoundResult());

        if (restaurant.OwnerId != ownerId)
            return (null, new ForbidResult());

        return (restaurant, null);
    }

    public static async Task<(Meal? Meal, IActionResult? Error)> RequireOwnedMealAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        int restaurantId,
        int mealId)
    {
        var (restaurant, restaurantError) = await RequireOwnedRestaurantAsync(db, user, restaurantId);
        if (restaurantError is not null)
            return (null, restaurantError);

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == mealId && m.RestaurantId == restaurantId);
        if (meal is null)
            return (null, new NotFoundResult());

        return (meal, null);
    }
}
