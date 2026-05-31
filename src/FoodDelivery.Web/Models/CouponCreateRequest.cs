namespace FoodDelivery.Web.Models;

public record CouponCreateRequest(
  string Code,
  decimal DiscountValue,
  DateTime ExpiresAt
);
