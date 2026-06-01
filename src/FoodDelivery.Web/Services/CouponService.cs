using System.Net.Http.Json;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class CouponService(HttpClient http)
{
    public async Task<List<CouponDto>> GetByRestaurantAsync(int restaurantId)
    {
        return await http.GetFromJsonAsync<List<CouponDto>>($"api/restaurants/{restaurantId}/coupons") ?? [];
    }

    public async Task<CouponValidationDto?> ValidateAsync(int restaurantId, string code, decimal subtotal)
    {
        var url =
            $"api/restaurants/{restaurantId}/coupon/validate?code={Uri.EscapeDataString(code.Trim())}&subtotal={subtotal}";
        return await http.GetFromJsonAsync<CouponValidationDto>(url);
    }

    public async Task<(bool Success, CouponDto? Coupon, string? Error)> CreateAsync(int restaurantId, CouponCreateRequest request)
    {
        var response = await http.PostAsJsonAsync($"api/restaurants/{restaurantId}/coupons", request);
        if (response.IsSuccessStatusCode)
        {
            var coupon = await response.Content.ReadFromJsonAsync<CouponDto>();
            return (true, coupon, null);
        }

        return (false, null, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> RemoveAsync(int restaurantId, int couponId)
    {
        var response = await http.DeleteAsync($"api/restaurants/{restaurantId}/coupons/{couponId}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }
}
