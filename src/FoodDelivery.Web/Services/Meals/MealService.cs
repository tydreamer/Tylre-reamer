using System.Net.Http.Json;
using FoodDelivery.Web.Helpers;

namespace FoodDelivery.Web.Services.Meals;

public class MealService(HttpClient http)
{
    public async Task<PagedResult<MealBrowseDto>?> GetPageAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        IEnumerable<int>? mealTypeIds = null)
    {
        var url = BrowseQueryHelper.BuildPagedUrl(
            "api/meals", page, pageSize, search, mealTypeIds, "mealTypeIds");

        return await http.GetBrowseJsonAsync<PagedResult<MealBrowseDto>>(url);
    }

    public async Task<MealBrowseDto?> GetBrowseByIdAsync(int mealId)
    {
        return await http.GetFromJsonAsync<MealBrowseDto>($"api/meals/{mealId}");
    }

    public async Task<List<MealTypeDto>> GetMealTypesAsync()
    {
        return await http.GetFromJsonAsync<List<MealTypeDto>>("api/meal-types") ?? [];
    }

    public async Task<List<MealDto>> GetByRestaurantAsync(int restaurantId)
    {
        return await http.GetFromJsonAsync<List<MealDto>>($"api/restaurants/{restaurantId}/meals") ?? [];
    }

    public async Task<MealDto?> GetByIdAsync(int restaurantId, int mealId)
    {
        return await http.GetFromJsonAsync<MealDto>($"api/restaurants/{restaurantId}/meals/{mealId}");
    }

    public Task<Result<MealDto>> CreateAsync(int restaurantId, MealCreateRequest request) =>
        http.PostForResultAsync<MealDto>($"api/restaurants/{restaurantId}/meals", request);

    public Task<Result> UpdateAsync(int restaurantId, int mealId, MealUpdateRequest request) =>
        http.PutForResultAsync($"api/restaurants/{restaurantId}/meals/{mealId}", request);

    public Task<Result> RemoveAsync(int restaurantId, int mealId) =>
        http.DeleteForResultAsync($"api/restaurants/{restaurantId}/meals/{mealId}");
}
