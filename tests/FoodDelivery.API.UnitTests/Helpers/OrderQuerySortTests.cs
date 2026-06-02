using FluentAssertions;
using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.UnitTests.Helpers;

public class OrderQuerySortTests
{
    [Fact]
    public async Task Apply_SortByTotalDescending_OrdersCorrectly()
    {
        await using var db = await CreateDbWithOrdersAsync();

        var ids = await OrderQuerySort.Apply(db.Orders.Include(o => o.Restaurant).Include(o => o.Customer).Include(o => o.Items), "total", sortDesc: true)
            .Select(o => o.Id)
            .ToListAsync();

        ids.Should().Equal(3, 1, 2);
    }

    [Fact]
    public async Task Apply_UnknownSortKey_DefaultsToDateDescending()
    {
        await using var db = await CreateDbWithOrdersAsync();

        var ids = await OrderQuerySort.Apply(db.Orders.Include(o => o.Restaurant).Include(o => o.Customer).Include(o => o.Items), "not-a-column", sortDesc: false)
            .Select(o => o.Id)
            .ToListAsync();

        ids.Should().Equal(3, 2, 1);
    }

    [Fact]
    public async Task Apply_SortByRestaurantNameAscending_OrdersAlphabetically()
    {
        await using var db = await CreateDbWithOrdersAsync();

        var names = await OrderQuerySort.Apply(db.Orders.Include(o => o.Restaurant).Include(o => o.Customer).Include(o => o.Items), "restaurant", sortDesc: false)
            .Select(o => o.Restaurant.Name)
            .ToListAsync();

        names.Should().Equal("Alpha Bistro", "Beta Kitchen", "Zulu Grill");
    }

    private static async Task<AppDbContext> CreateDbWithOrdersAsync()
    {
        var db = TestDbContextFactory.Create();

        var owner = new User { Id = 1, Email = "owner@test.com", Name = "Owner", PasswordHash = "x", Role = UserRole.Owner };
        var customer = new User { Id = 2, Email = "cust@test.com", Name = "Zara", PasswordHash = "x", Role = UserRole.Customer };
        var cuisine = new Cuisine { Id = 1, Name = "Italian" };

        var r1 = new Restaurant { Id = 1, Name = "Zulu Grill", Description = "", CuisineId = 1, OwnerId = 1, Latitude = 0, Longitude = 0 };
        var r2 = new Restaurant { Id = 2, Name = "Alpha Bistro", Description = "", CuisineId = 1, OwnerId = 1, Latitude = 0, Longitude = 0 };
        var r3 = new Restaurant { Id = 3, Name = "Beta Kitchen", Description = "", CuisineId = 1, OwnerId = 1, Latitude = 0, Longitude = 0 };

        db.Users.AddRange(owner, customer);
        db.Cuisines.Add(cuisine);
        db.Restaurants.AddRange(r1, r2, r3);

        db.Orders.AddRange(
            new Order
            {
                Id = 1,
                CustomerId = 2,
                RestaurantId = 2,
                TotalPrice = 25,
                CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                Items = [new OrderItem { Quantity = 2 }]
            },
            new Order
            {
                Id = 2,
                CustomerId = 2,
                RestaurantId = 3,
                TotalPrice = 10,
                CreatedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
                Items = [new OrderItem { Quantity = 1 }]
            },
            new Order
            {
                Id = 3,
                CustomerId = 2,
                RestaurantId = 1,
                TotalPrice = 99,
                CreatedAt = new DateTime(2026, 1, 3, 12, 0, 0, DateTimeKind.Utc),
                Items = [new OrderItem { Quantity = 5 }]
            });

        await db.SaveChangesAsync();
        return db;
    }
}
