namespace FoodDelivery.Web.Models.Coupons;

public record CouponValidationDto(
    bool IsValid,
    string? ErrorMessage,
    string? SuccessMessage,
    decimal DiscountAmount,
    decimal DiscountPercent,
    decimal DiscountedSubtotal);
