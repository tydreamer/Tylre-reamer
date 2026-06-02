using System.ComponentModel.DataAnnotations;
using static FoodDelivery.API.Constants.ValidationMessages;

namespace FoodDelivery.API.DTOs.Orders;

public record OrderItemRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Invalid meal.")] int MealId,
    [Range(1, 100, ErrorMessage = QuantityInvalid)] int Quantity
);

public record PlaceOrderRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Invalid restaurant.")] int RestaurantId,
    [Required(ErrorMessage = ItemsRequired), MinLength(1, ErrorMessage = ItemsRequired)] List<OrderItemRequest> Items,
    [Range(0, 100_000, ErrorMessage = TipInvalid)] decimal Tip,
    string? CouponCode
);

public record UpdateOrderStatusRequest(
    [Required(ErrorMessage = StatusRequired)] string Status
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
    List<OrderStatusHistoryResponse> StatusHistory,
    bool CustomerBlockedFromOwner = false);
