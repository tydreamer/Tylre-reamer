using System.Net.Http.Json;

namespace FoodDelivery.Web.Services.Coupons;

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

    public Task<Result<CouponDto>> CreateAsync(int restaurantId, CouponCreateRequest request) =>
        http.PostForResultAsync<CouponDto>($"api/restaurants/{restaurantId}/coupons", request);

    public Task<Result> RemoveAsync(int restaurantId, int couponId) =>
        http.DeleteForResultAsync($"api/restaurants/{restaurantId}/coupons/{couponId}");
}
