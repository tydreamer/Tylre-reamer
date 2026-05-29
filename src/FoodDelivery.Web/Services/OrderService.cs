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
}
