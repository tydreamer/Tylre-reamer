using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Helpers;

public enum CouponValidationStatus
{
    Valid,
    Invalid,
    Expired
}

public sealed record CouponValidationResult(
    CouponValidationStatus Status,
    Coupon? Coupon,
    decimal DiscountAmount)
{
    public static CouponValidationResult Invalid() =>
        new(CouponValidationStatus.Invalid, null, 0);

    public static CouponValidationResult Expired(Coupon coupon) =>
        new(CouponValidationStatus.Expired, coupon, 0);

    public static CouponValidationResult Valid(Coupon coupon, decimal discountAmount) =>
        new(CouponValidationStatus.Valid, coupon, discountAmount);
}

public static class CouponValidator
{
    public static async Task<CouponValidationResult> ValidateAsync(
        AppDbContext db,
        int restaurantId,
        string code,
        decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(code))
            return CouponValidationResult.Invalid();

        var normalizedCode = code.Trim();
        var coupon = await db.Coupons.FirstOrDefaultAsync(c =>
            c.Code == normalizedCode && c.RestaurantId == restaurantId);

        if (coupon is null)
            return CouponValidationResult.Invalid();

        if (coupon.ExpiresAt <= DateTime.UtcNow)
            return CouponValidationResult.Expired(coupon);

        if (!coupon.IsActive)
            return CouponValidationResult.Invalid();

        var discount = CalculateDiscount(coupon, subtotal);
        return CouponValidationResult.Valid(coupon, discount);
    }

    public static decimal CalculateDiscount(Coupon coupon, decimal subtotal) =>
        coupon.DiscountType == DiscountType.Percentage
            ? subtotal * coupon.DiscountValue / 100
            : coupon.DiscountValue;

    public static string ErrorMessage(CouponValidationStatus status) => status switch
    {
        CouponValidationStatus.Expired => "Expired coupon.",
        CouponValidationStatus.Invalid => "Invalid coupon.",
        _ => ""
    };

    public static string SuccessMessage(Coupon coupon) =>
        coupon.DiscountType == DiscountType.Percentage
            ? $"You're eligible for a {coupon.DiscountValue:0.##}% discount on your order!"
            : $"You're eligible for a {coupon.DiscountValue:C} discount on your order!";
}
