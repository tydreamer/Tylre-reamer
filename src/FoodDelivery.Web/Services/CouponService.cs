using System.Net.Http.Json;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class CouponService(HttpClient http)
{
    public async Task<List<CouponDto>> GetByRestaurantAsync(int restaurantId)
    {
        return await http.GetFromJsonAsync<List<CouponDto>>($"api/restaurants/{restaurantId}/coupons") ?? [];
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

    public async Task<(bool Success, string? Error)> DeleteAsync(int restaurantId, int couponId)
    {
        var response = await http.DeleteAsync($"api/restaurants/{restaurantId}/coupons/{couponId}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }
}
