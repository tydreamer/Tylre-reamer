namespace FoodDelivery.Web.Models;

public record CouponDto(int Id, string Code, string DiscountType, decimal DiscountValue, DateTime ExpiresAt, bool IsActive);
