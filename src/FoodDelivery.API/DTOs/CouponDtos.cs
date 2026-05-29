namespace FoodDelivery.API.DTOs;

public record CreateCouponRequest(string Code, string DiscountType, decimal DiscountValue, DateTime ExpiresAt);

public record CouponResponse(int Id, string Code, string DiscountType, decimal DiscountValue, DateTime ExpiresAt, bool IsActive);
