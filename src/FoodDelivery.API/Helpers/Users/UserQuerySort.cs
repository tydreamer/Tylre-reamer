using FoodDelivery.API.Models;

namespace FoodDelivery.API.Helpers.Users;

public static class UserQuerySort
{
    public static IQueryable<User> Apply(IQueryable<User> query, string? sortBy, bool sortDesc)
    {
        var key = sortBy?.Trim().ToLowerInvariant();

        return (key, sortDesc) switch
        {
            ("name", false) => query.OrderBy(u => u.Name),
            ("name", true) => query.OrderByDescending(u => u.Name),

            ("email", false) => query.OrderBy(u => u.Email),
            ("email", true) => query.OrderByDescending(u => u.Email),

            ("role", false) => query.OrderBy(u => u.Role),
            ("role", true) => query.OrderByDescending(u => u.Role),

            ("status", false) => query.OrderBy(u => u.IsBlocked),
            ("status", true) => query.OrderByDescending(u => u.IsBlocked),

            _ => query.OrderBy(u => u.Name)
        };
    }
}
