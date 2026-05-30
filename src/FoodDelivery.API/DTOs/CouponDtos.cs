using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateCouponRequest(
    [property: Required, MaxLength(50)] string Code,
    [property: Required] string DiscountType,
    [property: Range(0.01, 100_000)] decimal DiscountValue,
    DateTime ExpiresAt
);

public record CouponResponse(int Id, string Code, string DiscountType, decimal DiscountValue, DateTime ExpiresAt, bool IsActive);
