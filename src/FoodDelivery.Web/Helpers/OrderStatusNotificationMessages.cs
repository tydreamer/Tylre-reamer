using FoodDelivery.Web.Models.Orders;

namespace FoodDelivery.Web.Helpers;

public static class OrderStatusNotificationMessages
{
    public static string Format(OrderStatusNotification notification, bool isOwner) =>
        isOwner ? ForOwner(notification) : ForCustomer(notification);

    private static string ForOwner(OrderStatusNotification n) => n.Status switch
    {
        "Placed" => $"New order #{n.OrderId} at {n.RestaurantName}",
        "Processing" => $"Order #{n.OrderId} is being prepared",
        "InRoute" => $"Order #{n.OrderId} is on the way",
        "Delivered" => $"Order #{n.OrderId} was delivered",
        "Received" => $"Order #{n.OrderId} was received by the customer",
        "Cancelled" => $"Order #{n.OrderId} was cancelled",
        _ => $"Order #{n.OrderId} updated to {n.Status}"
    };

    private static string ForCustomer(OrderStatusNotification n) => n.Status switch
    {
        "Placed" => $"Your order at {n.RestaurantName} was placed",
        "Processing" => $"{n.RestaurantName} is preparing your order",
        "InRoute" => $"Your order from {n.RestaurantName} is on the way",
        "Delivered" => $"Your order from {n.RestaurantName} was delivered",
        "Received" => $"Order #{n.OrderId} marked as received",
        "Cancelled" => $"Your order from {n.RestaurantName} was cancelled",
        _ => $"Order #{n.OrderId} updated to {n.Status}"
    };
}
