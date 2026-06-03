using FoodDelivery.API.Data;
using FoodDelivery.API.Helpers.Coupons;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.Services.Orders;

public sealed record PricedOrder(decimal Subtotal, decimal Discount, decimal Total, Coupon? Coupon, CouponValidationStatus? CouponStatus = null);

public class OrderPricingService(AppDbContext db)
{
    public async Task<PricedOrder> PriceAsync(
        IReadOnlyList<(int MealId, int Quantity)> items,
        IReadOnlyDictionary<int, decimal> mealPrices,
        int restaurantId,
        decimal tip,
        string? couponCode)
    {
        var subtotal = items.Sum(i => mealPrices[i.MealId] * i.Quantity);
        decimal discount = 0;
        Coupon? coupon = null;
        CouponValidationStatus? couponStatus = null;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var v = await CouponValidator.ValidateAsync(db, restaurantId, couponCode, subtotal);
            couponStatus = v.Status;
            if (v.Status == CouponValidationStatus.Valid && v.Coupon is not null)
                (coupon, discount) = (v.Coupon, v.DiscountAmount);
        }

        return new PricedOrder(subtotal, discount, subtotal - discount + tip, coupon, couponStatus);
    }
}
