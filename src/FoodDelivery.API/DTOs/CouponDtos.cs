using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateCouponRequest(
    [Required, MaxLength(50)] string Code,
    [Required] string DiscountType,
    [Range(0.01, 100_000)] decimal DiscountValue,
    DateTime ExpiresAt
);

public record CouponResponse(int Id, string Code, string DiscountType, decimal DiscountValue, DateTime ExpiresAt, bool IsActive);
