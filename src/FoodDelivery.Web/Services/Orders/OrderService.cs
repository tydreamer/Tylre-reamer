using System.Net.Http.Json;

namespace FoodDelivery.Web.Services.Orders;

public class OrderService(HttpClient http)
{
    public Task<Result<OrderResponse>> PlaceOrderAsync(PlaceOrderRequest request) =>
        http.PostForResultAsync<OrderResponse>("api/orders", request);

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

    public Task<Result> UpdateStatusAsync(int id, string status) =>
        http.PutForResultAsync($"api/orders/{id}/status", new { Status = status });
}
