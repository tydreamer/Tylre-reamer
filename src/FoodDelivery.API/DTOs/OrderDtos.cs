using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public record OrderItemRequest(
    [Range(1, int.MaxValue)] int MealId,
    [Range(1, 100)] int Quantity
);

public record PlaceOrderRequest(
    [Range(1, int.MaxValue)] int RestaurantId,
    [Required, MinLength(1)] List<OrderItemRequest> Items,
    [Range(0, 100_000)] decimal Tip,
    [MaxLength(50)] string? CouponCode
);

public record UpdateOrderStatusRequest(
    [Required] string Status
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
