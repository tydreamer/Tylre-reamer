namespace FoodDelivery.API.DTOs;

public record CouponValidationResponse(
    bool IsValid,
    string? ErrorMessage,
    string? SuccessMessage,
    decimal DiscountAmount,
    decimal DiscountPercent,
    decimal DiscountedSubtotal);
