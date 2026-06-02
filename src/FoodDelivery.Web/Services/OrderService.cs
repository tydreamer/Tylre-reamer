using System.Net.Http.Json;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class OrderService(HttpClient http)
{
    public async Task<(bool Success, OrderResponse? Order, string? Error)> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var response = await http.PostAsJsonAsync("api/orders", request);
        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
            return (true, order, null);
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, null, error);
    }

    public async Task<PagedResult<OrderResponse>?> GetMyOrdersAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDesc = true)
    {
        var url = $"api/orders?page={page}&pageSize={pageSize}&sortDesc={sortDesc.ToString().ToLowerInvariant()}";
        if (!string.IsNullOrWhiteSpace(sortBy))
            url += $"&sortBy={Uri.EscapeDataString(sortBy)}";

        return await http.GetFromJsonAsync<PagedResult<OrderResponse>>(url);
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        return await http.GetFromJsonAsync<OrderResponse>($"api/orders/{id}");
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status)
    {
        var response = await http.PutAsJsonAsync($"api/orders/{id}/status", new { Status = status });
        if (response.IsSuccessStatusCode)
            return (true, null);

        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }
}
