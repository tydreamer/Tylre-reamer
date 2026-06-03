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

    public async Task<(bool Success, MealDto? Meal, string? Error)> CreateAsync(int restaurantId, MealCreateRequest request)
    {
        var response = await http.PostAsJsonAsync($"api/restaurants/{restaurantId}/meals", request);
        if (response.IsSuccessStatusCode)
        {
            var meal = await response.Content.ReadFromJsonAsync<MealDto>();
            return (true, meal, null);
        }

        return (false, null, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int restaurantId, int mealId, MealUpdateRequest request)
    {
        var response = await http.PutAsJsonAsync($"api/restaurants/{restaurantId}/meals/{mealId}", request);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> RemoveAsync(int restaurantId, int mealId)
    {
        var response = await http.DeleteAsync($"api/restaurants/{restaurantId}/meals/{mealId}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }
}
