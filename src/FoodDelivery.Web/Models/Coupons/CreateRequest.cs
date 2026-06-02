namespace FoodDelivery.Web.Models.Coupons;

public record CouponCreateRequest(
  string Code,
  decimal DiscountValue,
  DateTime ExpiresAt
);
