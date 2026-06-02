using FoodDelivery.API.Models;

namespace FoodDelivery.API.Helpers.Meals;

public static class MealMapper
{
    public static MealResponse ToResponse(Meal meal) =>
        new(
            meal.Id,
            meal.Name,
            meal.Description,
            meal.ImageUrl,
            meal.Price,
            meal.IsAvailable,
            meal.RestaurantId,
            meal.MealTypeId,
            meal.MealType.Name);

    public static MealBrowseResponse ToBrowseResponse(Meal meal) =>
        new(
            meal.Id,
            meal.Name,
            meal.Description,
            meal.ImageUrl,
            meal.Price,
            meal.IsAvailable,
            meal.RestaurantId,
            meal.Restaurant.Name,
            meal.MealTypeId,
            meal.MealType.Name);
}
