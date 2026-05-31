using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs;

public record CreateCouponRequest(
    [Required(ErrorMessage = CodeRequired), MaxLength(10, ErrorMessage = CodeTooLong)] string Code,
    [Range(1, 100, ErrorMessage = DiscountInvalid)] decimal DiscountValue,
    DateTime ExpiresAt
);

public record CouponResponse(int Id, string Code, decimal DiscountValue, DateTime ExpiresAt, bool IsActive);
