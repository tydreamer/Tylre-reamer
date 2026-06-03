using FoodDelivery.API.Data;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.IntegrationTests.Infrastructure;

public static class IntegrationTestIds
{
    public const int CustomerId = 1;
    public const int OwnerId = 2;
    public const int OwnerBlockedCustomerId = 3;
    public const int GloballyBlockedCustomerId = 4;

    public const int RestaurantId = 10;
    public const int AvailableMealId = 100;

    public const string ValidCouponCode = "SAVE10";
    public const string ExpiredCouponCode = "OLD";
}

public static class IntegrationTestSeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        db.Users.AddRange(
            new User
            {
                Id = IntegrationTestIds.CustomerId,
                Name = "Test Customer",
                Email = "customer@test.com",
                PasswordHash = "hash",
                Role = UserRole.Customer
            },
            new User
            {
                Id = IntegrationTestIds.OwnerId,
                Name = "Test Owner",
                Email = "owner@test.com",
                PasswordHash = "hash",
                Role = UserRole.Owner
            },
            new User
            {
                Id = IntegrationTestIds.OwnerBlockedCustomerId,
                Name = "Blocked Customer",
                Email = "blocked@test.com",
                PasswordHash = "hash",
                Role = UserRole.Customer
            },
            new User
            {
                Id = IntegrationTestIds.GloballyBlockedCustomerId,
                Name = "Global Block",
                Email = "globalblock@test.com",
                PasswordHash = "hash",
                Role = UserRole.Customer,
                IsBlocked = true
            });

        db.Cuisines.Add(new Cuisine { Id = 1, Name = "Italian" });
        db.MealTypes.AddRange(
            new MealType { Id = 1, Name = "Main" },
            new MealType { Id = 2, Name = "Dessert" });

        db.Restaurants.Add(new Restaurant
        {
            Id = IntegrationTestIds.RestaurantId,
            Name = "Test Bistro",
            Description = "Integration test restaurant",
            CuisineId = 1,
            OwnerId = IntegrationTestIds.OwnerId,
            Latitude = 40.7,
            Longitude = -74.0
        });

        db.Meals.AddRange(
            new Meal
            {
                Id = IntegrationTestIds.AvailableMealId,
                Name = "Burger",
                Description = "Tasty",
                Price = 10m,
                MealTypeId = 1,
                RestaurantId = IntegrationTestIds.RestaurantId
            },
            new Meal
            {
                Id = 101,
                Name = "Cake",
                Description = "Sweet",
                Price = 6m,
                MealTypeId = 2,
                RestaurantId = IntegrationTestIds.RestaurantId
            });

        db.Coupons.AddRange(
            new Coupon
            {
                Code = IntegrationTestIds.ValidCouponCode,
                RestaurantId = IntegrationTestIds.RestaurantId,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsActive = true
            },
            new Coupon
            {
                Code = IntegrationTestIds.ExpiredCouponCode,
                RestaurantId = IntegrationTestIds.RestaurantId,
                DiscountType = DiscountType.Fixed,
                DiscountValue = 5,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            });

        db.OwnerCustomerBlocks.Add(new OwnerCustomerBlock
        {
            OwnerId = IntegrationTestIds.OwnerId,
            CustomerId = IntegrationTestIds.OwnerBlockedCustomerId
        });

        db.Orders.AddRange(
            new Order
            {
                CustomerId = IntegrationTestIds.CustomerId,
                RestaurantId = IntegrationTestIds.RestaurantId,
                TotalPrice = 15m,
                Tip = 0m,
                Status = OrderStatus.Placed,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Items = [new OrderItem { MealId = IntegrationTestIds.AvailableMealId, Quantity = 1, UnitPrice = 15m }],
                StatusHistory = [new OrderStatusHistory { Status = OrderStatus.Placed }]
            },
            new Order
            {
                CustomerId = IntegrationTestIds.CustomerId,
                RestaurantId = IntegrationTestIds.RestaurantId,
                TotalPrice = 40m,
                Tip = 0m,
                Status = OrderStatus.Placed,
                CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                Items = [new OrderItem { MealId = IntegrationTestIds.AvailableMealId, Quantity = 4, UnitPrice = 10m }],
                StatusHistory = [new OrderStatusHistory { Status = OrderStatus.Placed }]
            });

        await db.SaveChangesAsync();
    }
}
