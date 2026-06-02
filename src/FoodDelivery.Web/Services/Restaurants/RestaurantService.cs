using System.Net.Http.Json;

namespace FoodDelivery.Web.Services.Restaurants;

public class RestaurantService(HttpClient http)
{
    public async Task<PagedResult<RestaurantDto>?> GetPageAsync(
        int page = 1,
        int pageSize = 12,
        int? ownerId = null,
        string? search = null)
    {
        var url = $"api/restaurants?page={page}&pageSize={pageSize}";
        if (ownerId.HasValue)
            url += $"&ownerId={ownerId.Value}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search.Trim())}";

        return await http.GetFromJsonAsync<PagedResult<RestaurantDto>>(url);
    }

    public async Task<RestaurantDto?> GetByIdAsync(int id)
    {
        return await http.GetFromJsonAsync<RestaurantDto>($"api/restaurants/{id}");
    }

    public async Task<List<CuisineDto>> GetCuisinesAsync()
    {
        return await http.GetFromJsonAsync<List<CuisineDto>>("api/cuisines") ?? [];
    }

    public async Task<List<RestaurantDto>> GetByOwnerAsync(int ownerId)
    {
        var paged = await GetPageAsync(1, 100, ownerId);
        return paged?.Items ?? [];
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

    public async Task<(bool Success, string? Error)> RemoveAsync(int id)
    {
        var response = await http.DeleteAsync($"api/restaurants/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }   
}
