using FoodDelivery.API.Models;

namespace FoodDelivery.API.Helpers;

public static class OrderQuerySort
{
    public static IQueryable<Order> Apply(IQueryable<Order> query, string? sortBy, bool sortDesc)
    {
        var key = sortBy?.Trim().ToLowerInvariant();

        return (key, sortDesc) switch
        {
            ("id", false) => query.OrderBy(o => o.Id),
            ("id", true) => query.OrderByDescending(o => o.Id),

            ("restaurant", false) => query.OrderBy(o => o.Restaurant.Name),
            ("restaurant", true) => query.OrderByDescending(o => o.Restaurant.Name),

            ("customer", false) => query.OrderBy(o => o.Customer.Name),
            ("customer", true) => query.OrderByDescending(o => o.Customer.Name),

            ("items", false) => query.OrderBy(o => o.Items.Sum(i => i.Quantity)),
            ("items", true) => query.OrderByDescending(o => o.Items.Sum(i => i.Quantity)),

            ("date", false) => query.OrderBy(o => o.CreatedAt),
            ("date", true) => query.OrderByDescending(o => o.CreatedAt),

            ("total", false) => query.OrderBy(o => o.TotalPrice),
            ("total", true) => query.OrderByDescending(o => o.TotalPrice),

            ("status", false) => query.OrderBy(o => o.Status),
            ("status", true) => query.OrderByDescending(o => o.Status),

            _ => query.OrderByDescending(o => o.CreatedAt)
        };
    }
}
