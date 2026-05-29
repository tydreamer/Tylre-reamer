using System.Net.Http.Json;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class MealService(HttpClient http)
{
    public async Task<List<MealDto>> GetByRestaurantAsync(int restaurantId)
    {
        return await http.GetFromJsonAsync<List<MealDto>>($"api/restaurants/{restaurantId}/meals") ?? [];
    }
}
