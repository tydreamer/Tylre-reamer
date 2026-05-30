namespace FoodDelivery.Web.Models;

public record OrderItemRequest(int MealId, int Quantity);

public record PlaceOrderRequest(int RestaurantId, List<OrderItemRequest> Items, decimal Tip, string? CouponCode);

public record OrderItemResponse(int MealId, string MealName, int Quantity, decimal UnitPrice);

public record OrderStatusHistoryResponse(string Status, DateTime ChangedAt);

public record OrderResponse(
    int Id,
    int CustomerId,
    int RestaurantId,
    string RestaurantName,
    string Status,
    decimal Tip,
    decimal TotalPrice,
    DateTime CreatedAt,
    List<OrderItemResponse> Items,
    List<OrderStatusHistoryResponse> StatusHistory
);
