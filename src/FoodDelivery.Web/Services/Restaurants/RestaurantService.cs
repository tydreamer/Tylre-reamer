using System.Net.Http.Json;
using FoodDelivery.Web.Helpers;

namespace FoodDelivery.Web.Services.Restaurants;

public class RestaurantService(HttpClient http)
{
    public async Task<PagedResult<RestaurantDto>?> GetPageAsync(
        int page = 1,
        int pageSize = 12,
        int? ownerId = null,
        string? search = null,
        IEnumerable<int>? cuisineIds = null)
    {
        var url = BrowseQueryHelper.BuildPagedUrl(
            "api/restaurants", page, pageSize, search, cuisineIds, "cuisineIds", ownerId);

        return await http.GetBrowseJsonAsync<PagedResult<RestaurantDto>>(url);
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

    public Task<Result<RestaurantDto>> CreateAsync(RestaurantUpsertRequest request) =>
        http.PostForResultAsync<RestaurantDto>("api/restaurants", request);

    public Task<Result> UpdateAsync(int id, RestaurantUpsertRequest request) =>
        http.PutForResultAsync($"api/restaurants/{id}", request);

    public Task<Result> RemoveAsync(int id) =>
        http.DeleteForResultAsync($"api/restaurants/{id}");
}
