using FluentAssertions;
using FoodDelivery.API.Data;
using FoodDelivery.API.Helpers;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.UnitTests.Helpers;

public class CouponValidatorTests
{
    [Theory]
    [InlineData(DiscountType.Percentage, 10, 50, 5)]
    [InlineData(DiscountType.Percentage, 25, 80, 20)]
    [InlineData(DiscountType.Fixed, 7.50, 100, 7.50)]
    public void CalculateDiscount_ReturnsExpectedAmount(
        DiscountType type,
        decimal value,
        decimal subtotal,
        decimal expected)
    {
        var coupon = new Coupon { DiscountType = type, DiscountValue = value };

        CouponValidator.CalculateDiscount(coupon, subtotal).Should().Be(expected);
    }

    [Theory]
    [InlineData(CouponValidationStatus.Expired, "Expired coupon.")]
    [InlineData(CouponValidationStatus.Invalid, "Invalid coupon.")]
    [InlineData(CouponValidationStatus.Valid, "")]
    public void ErrorMessage_ReturnsExpectedText(CouponValidationStatus status, string expected)
    {
        CouponValidator.ErrorMessage(status).Should().Be(expected);
    }

    [Fact]
    public void SuccessMessage_Percentage_IncludesPercentValue()
    {
        var coupon = new Coupon { DiscountType = DiscountType.Percentage, DiscountValue = 15 };

        CouponValidator.SuccessMessage(coupon).Should().Contain("15%");
    }

    [Fact]
    public async Task ValidateAsync_EmptyCode_ReturnsInvalid()
    {
        await using var db = TestDbContextFactory.Create();

        var result = await CouponValidator.ValidateAsync(db, restaurantId: 1, code: "  ", subtotal: 50);

        result.Status.Should().Be(CouponValidationStatus.Invalid);
        result.Coupon.Should().BeNull();
        result.DiscountAmount.Should().Be(0);
    }

    [Fact]
    public async Task ValidateAsync_UnknownCode_ReturnsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        SeedRestaurant(db, restaurantId: 1);

        var result = await CouponValidator.ValidateAsync(db, 1, "MISSING", 50);

        result.Status.Should().Be(CouponValidationStatus.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_ExpiredCoupon_ReturnsExpired()
    {
        await using var db = TestDbContextFactory.Create();
        SeedRestaurant(db, 1);
        db.Coupons.Add(new Coupon
        {
            Code = "OLD10",
            RestaurantId = 1,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var result = await CouponValidator.ValidateAsync(db, 1, "OLD10", 100);

        result.Status.Should().Be(CouponValidationStatus.Expired);
        result.Coupon.Should().NotBeNull();
        result.DiscountAmount.Should().Be(0);
    }

    [Fact]
    public async Task ValidateAsync_InactiveCoupon_ReturnsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        SeedRestaurant(db, 1);
        db.Coupons.Add(new Coupon
        {
            Code = "OFF",
            RestaurantId = 1,
            DiscountType = DiscountType.Fixed,
            DiscountValue = 5,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = false
        });
        await db.SaveChangesAsync();

        var result = await CouponValidator.ValidateAsync(db, 1, "OFF", 100);

        result.Status.Should().Be(CouponValidationStatus.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_ValidPercentage_ReturnsDiscount()
    {
        await using var db = TestDbContextFactory.Create();
        SeedRestaurant(db, 1);
        db.Coupons.Add(new Coupon
        {
            Code = "SAVE20",
            RestaurantId = 1,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var result = await CouponValidator.ValidateAsync(db, 1, "SAVE20", 50);

        result.Status.Should().Be(CouponValidationStatus.Valid);
        result.DiscountAmount.Should().Be(10);
    }

    [Fact]
    public async Task ValidateAsync_WrongRestaurant_ReturnsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        SeedRestaurant(db, 1);
        SeedRestaurant(db, 2, ownerId: 2);
        db.Coupons.Add(new Coupon
        {
            Code = "R1ONLY",
            RestaurantId = 1,
            DiscountType = DiscountType.Fixed,
            DiscountValue = 5,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var result = await CouponValidator.ValidateAsync(db, 2, "R1ONLY", 50);

        result.Status.Should().Be(CouponValidationStatus.Invalid);
    }

    private static void SeedRestaurant(AppDbContext db, int restaurantId, int ownerId = 1)
    {
        if (!db.Users.Any(u => u.Id == ownerId))
        {
            db.Users.Add(new User
            {
                Id = ownerId,
                Email = $"owner{ownerId}@test.com",
                Name = $"Owner {ownerId}",
                PasswordHash = "hash",
                Role = UserRole.Owner
            });
        }

        if (!db.Cuisines.Any(c => c.Id == 1))
        {
            db.Cuisines.Add(new Cuisine { Id = 1, Name = "Italian" });
            db.SaveChanges();
        }

        if (!db.Restaurants.Any(r => r.Id == restaurantId))
        {
            db.Restaurants.Add(new Restaurant
            {
                Id = restaurantId,
                Name = $"Restaurant {restaurantId}",
                Description = "Test",
                CuisineId = 1,
                OwnerId = ownerId,
                Latitude = 0,
                Longitude = 0
            });
            db.SaveChanges();
        }
    }
}
