namespace FoodDelivery.Web.Models.Coupons;

public record CouponDto(
  int Id,
  string Code,
  decimal DiscountValue,
  DateTime ExpiresAt,
  bool IsActive
);
