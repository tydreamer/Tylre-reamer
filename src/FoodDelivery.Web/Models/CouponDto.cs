namespace FoodDelivery.Web.Models;

public record CouponDto(
  int Id,
  string Code,
  decimal DiscountValue,
  DateTime ExpiresAt,
  bool IsActive
);
