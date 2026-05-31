using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateCouponRequest(
    [Required, MaxLength(10)] string Code,
    [Range(1, 100)] decimal DiscountValue,
    DateTime ExpiresAt
);

public record CouponResponse(
    int Id,
    string Code,
    decimal DiscountValue,
    DateTime ExpiresAt,
    bool IsActive
);
