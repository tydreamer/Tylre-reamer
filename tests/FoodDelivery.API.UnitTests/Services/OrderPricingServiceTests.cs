using FluentAssertions;
using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services.Orders;

namespace FoodDelivery.API.UnitTests.Services;

public class OrderPricingServiceTests
{
    [Fact]
    public async Task PriceAsync_BasicItems_ComputesSubtotalAndTotal()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 2), (2, 1)],
            new Dictionary<int, decimal> { [1] = 10m, [2] = 8m },
            restaurantId: 1, tip: 0m, couponCode: null);

        result.Subtotal.Should().Be(28m); // 10×2 + 8×1
        result.Discount.Should().Be(0m);
        result.Total.Should().Be(28m);
        result.Coupon.Should().BeNull();
        result.CouponStatus.Should().BeNull();
    }

    [Fact]
    public async Task PriceAsync_WithTip_AddsTipToTotal()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 1)],
            new Dictionary<int, decimal> { [1] = 20m },
            restaurantId: 1, tip: 5m, couponCode: null);

        result.Subtotal.Should().Be(20m);
        result.Total.Should().Be(25m);
    }

    [Fact]
    public async Task PriceAsync_NullCouponCode_NoDiscount()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 1)],
            new Dictionary<int, decimal> { [1] = 20m },
            restaurantId: 1, tip: 0m, couponCode: null);

        result.Discount.Should().Be(0m);
        result.CouponStatus.Should().BeNull();
    }

    [Fact]
    public async Task PriceAsync_ValidPercentageCoupon_AppliesDiscount()
    {
        await using var db = TestDbContextFactory.Create();
        SeedCoupon(db, restaurantId: 1, code: "SAVE10", DiscountType.Percentage, 10m);
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 2)],
            new Dictionary<int, decimal> { [1] = 10m },
            restaurantId: 1, tip: 0m, couponCode: "SAVE10");

        result.Subtotal.Should().Be(20m);
        result.Discount.Should().Be(2m); // 10% of 20
        result.Total.Should().Be(18m);
        result.Coupon.Should().NotBeNull();
        result.CouponStatus.Should().Be(CouponValidationStatus.Valid);
    }

    [Fact]
    public async Task PriceAsync_ValidFixedCouponWithTip_TipNotDiscounted()
    {
        await using var db = TestDbContextFactory.Create();
        SeedCoupon(db, restaurantId: 1, code: "FLAT5", DiscountType.Fixed, 5m);
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 1)],
            new Dictionary<int, decimal> { [1] = 30m },
            restaurantId: 1, tip: 3m, couponCode: "FLAT5");

        result.Subtotal.Should().Be(30m);
        result.Discount.Should().Be(5m);
        result.Total.Should().Be(28m); // 30 - 5 + 3 tip
    }

    [Fact]
    public async Task PriceAsync_ExpiredCoupon_ZeroDiscountAndStatusSet()
    {
        await using var db = TestDbContextFactory.Create();
        SeedCoupon(db, restaurantId: 1, code: "OLD", DiscountType.Percentage, 20m, expired: true);
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 1)],
            new Dictionary<int, decimal> { [1] = 20m },
            restaurantId: 1, tip: 0m, couponCode: "OLD");

        result.Discount.Should().Be(0m);
        result.Total.Should().Be(20m);
        result.Coupon.Should().BeNull();
        result.CouponStatus.Should().Be(CouponValidationStatus.Expired);
    }

    [Fact]
    public async Task PriceAsync_InvalidCouponCode_ZeroDiscountAndStatusSet()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 1)],
            new Dictionary<int, decimal> { [1] = 20m },
            restaurantId: 1, tip: 0m, couponCode: "BOGUS");

        result.Discount.Should().Be(0m);
        result.Coupon.Should().BeNull();
        result.CouponStatus.Should().Be(CouponValidationStatus.Invalid);
    }

    [Fact]
    public async Task PriceAsync_MultipleItemsMultipleQuantities_SubtotalCorrect()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new OrderPricingService(db);

        var result = await service.PriceAsync(
            [(1, 3), (2, 2), (3, 1)],
            new Dictionary<int, decimal> { [1] = 5m, [2] = 8m, [3] = 12m },
            restaurantId: 1, tip: 0m, couponCode: null);

        result.Subtotal.Should().Be(43m); // 5×3 + 8×2 + 12×1
    }

    private static void SeedCoupon(
        AppDbContext db,
        int restaurantId,
        string code,
        DiscountType type,
        decimal value,
        bool expired = false)
    {
        if (!db.Users.Any(u => u.Id == 1))
            db.Users.Add(new User { Id = 1, Email = "owner@test.com", Name = "Owner", PasswordHash = "x", Role = UserRole.Owner });

        if (!db.Cuisines.Any(c => c.Id == 1))
            db.Cuisines.Add(new Cuisine { Id = 1, Name = "Italian" });

        db.SaveChanges();

        if (!db.Restaurants.Any(r => r.Id == restaurantId))
            db.Restaurants.Add(new Restaurant { Id = restaurantId, Name = "Test", Description = "Test", CuisineId = 1, OwnerId = 1 });

        db.Coupons.Add(new Coupon
        {
            Code = code,
            RestaurantId = restaurantId,
            DiscountType = type,
            DiscountValue = value,
            ExpiresAt = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(30),
            IsActive = true
        });

        db.SaveChanges();
    }
}
