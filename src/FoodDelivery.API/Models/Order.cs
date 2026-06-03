namespace FoodDelivery.API.Models;

public enum OrderStatus { Placed, Cancelled, Processing, InRoute, Delivered, Received }

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public decimal Tip { get; set; }
    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int MealId { get; set; }
    public Meal Meal { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
