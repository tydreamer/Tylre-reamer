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

    public async Task<List<RestaurantDto>> GetByOwnerAsync(int ownerId)
    {
        var all = await GetAllAsync();
        return all.Where(r => r.OwnerId == ownerId).ToList();
    }

    public async Task<(bool Success, RestaurantDto? Restaurant, string? Error)> CreateAsync(RestaurantUpsertRequest request)
    {
        var response = await http.PostAsJsonAsync("api/restaurants", request);
        if (response.IsSuccessStatusCode)
        {
            var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>();
            return (true, restaurant, null);
        }

        return (false, null, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, RestaurantUpsertRequest request)
    {
        var response = await http.PutAsJsonAsync($"api/restaurants/{id}", request);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/restaurants/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }
}
