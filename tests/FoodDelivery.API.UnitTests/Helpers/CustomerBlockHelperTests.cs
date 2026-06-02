using FluentAssertions;
using FoodDelivery.API.Helpers;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.UnitTests.Helpers;

public class CustomerBlockHelperTests
{
    [Fact]
    public async Task IsBlockedFromOwnerAsync_WhenBlockExists_ReturnsTrue()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(
            new User { Id = 1, Email = "o@test.com", Name = "Owner", PasswordHash = "x", Role = UserRole.Owner },
            new User { Id = 2, Email = "c@test.com", Name = "Customer", PasswordHash = "x", Role = UserRole.Customer });
        db.OwnerCustomerBlocks.Add(new OwnerCustomerBlock { OwnerId = 1, CustomerId = 2 });
        await db.SaveChangesAsync();

        var blocked = await CustomerBlockHelper.IsBlockedFromOwnerAsync(db, 1, 2);

        blocked.Should().BeTrue();
    }

    [Fact]
    public async Task IsBlockedFromOwnerAsync_WhenNoBlock_ReturnsFalse()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(
            new User { Id = 1, Email = "o@test.com", Name = "Owner", PasswordHash = "x", Role = UserRole.Owner },
            new User { Id = 2, Email = "c@test.com", Name = "Customer", PasswordHash = "x", Role = UserRole.Customer });
        await db.SaveChangesAsync();

        var blocked = await CustomerBlockHelper.IsBlockedFromOwnerAsync(db, 1, 2);

        blocked.Should().BeFalse();
    }

    [Fact]
    public async Task HasOrderedFromOwnerAsync_WhenOrderExists_ReturnsTrue()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(
            new User { Id = 1, Email = "o@test.com", Name = "Owner", PasswordHash = "x", Role = UserRole.Owner },
            new User { Id = 2, Email = "c@test.com", Name = "Customer", PasswordHash = "x", Role = UserRole.Customer });
        db.Cuisines.Add(new Cuisine { Id = 1, Name = "Italian" });
        db.Restaurants.Add(new Restaurant
        {
            Id = 10,
            Name = "Place",
            Description = "",
            CuisineId = 1,
            OwnerId = 1,
            Latitude = 0,
            Longitude = 0
        });
        db.Orders.Add(new Order { CustomerId = 2, RestaurantId = 10, TotalPrice = 20 });
        await db.SaveChangesAsync();

        var hasOrdered = await CustomerBlockHelper.HasOrderedFromOwnerAsync(db, 1, 2);

        hasOrdered.Should().BeTrue();
    }
}
