using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record CreateCouponRequest(
    [Required(ErrorMessage = "Code is required."),
     MaxLength(10, ErrorMessage = "Code cannot exceed 10 characters.")] string Code,
    [Range(1, 100, ErrorMessage = "Discount must be between 1% and 100%.")] decimal DiscountValue,
    DateTime ExpiresAt
);

public record CouponResponse(int Id, string Code, decimal DiscountValue, DateTime ExpiresAt, bool IsActive);
