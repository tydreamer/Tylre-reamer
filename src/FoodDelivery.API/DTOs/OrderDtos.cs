using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record OrderItemRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Invalid meal.")] int MealId,
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")] int Quantity
);

public record PlaceOrderRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Invalid restaurant.")] int RestaurantId,
    [Required(ErrorMessage = "At least one item is required."),
     MinLength(1, ErrorMessage = "At least one item is required.")] List<OrderItemRequest> Items,
    [Range(0, 100_000, ErrorMessage = "Tip must be 0 or greater.")] decimal Tip,
    [MaxLength(10, ErrorMessage = "Coupon code cannot exceed 10 characters.")] string? CouponCode
);

public record UpdateOrderStatusRequest(
    [Required(ErrorMessage = "Status is required.")] string Status
);

public record OrderStatusNotification(int OrderId, string RestaurantName, string Status);

public record OrderItemResponse(int MealId, string MealName, int Quantity, decimal UnitPrice);

public record OrderStatusHistoryResponse(string Status, DateTime ChangedAt);

public record OrderResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    int RestaurantId,
    string RestaurantName,
    string Status,
    decimal Tip,
    decimal TotalPrice,
    DateTime CreatedAt,
    List<OrderItemResponse> Items,
    List<OrderStatusHistoryResponse> StatusHistory);
