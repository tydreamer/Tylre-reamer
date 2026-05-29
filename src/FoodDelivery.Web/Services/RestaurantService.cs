using System.Net.Http.Json;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class RestaurantService(HttpClient http)
{
    public async Task<List<RestaurantDto>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<RestaurantDto>>("api/restaurants") ?? [];
    }

    public async Task<RestaurantDto?> GetByIdAsync(int id)
    {
        return await http.GetFromJsonAsync<RestaurantDto>($"api/restaurants/{id}");
    }
}
