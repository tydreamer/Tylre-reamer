namespace FoodDelivery.API.Models;

public enum DiscountType { Fixed, Percentage }

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
