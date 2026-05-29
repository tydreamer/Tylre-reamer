namespace FoodDelivery.Web.Models;

public record CouponCreateRequest(string Code, string DiscountType, decimal DiscountValue, DateTime ExpiresAt);
