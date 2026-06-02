using System.Net;
using FluentAssertions;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Coupons;

[Collection(IntegrationTestCollection.Name)]
public class CouponValidationApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Validate_ValidCoupon_ReturnsSuccess()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.GetAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/coupon/validate?code={IntegrationTestIds.ValidCouponCode}&subtotal=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadJsonAsync<CouponValidationResponse>();
        body.Should().NotBeNull();
        body!.IsValid.Should().BeTrue();
        body.DiscountAmount.Should().Be(5m);
        body.SuccessMessage.Should().Contain("10%");
    }

    [Fact]
    public async Task Validate_ExpiredCoupon_ReturnsInvalidWithMessage()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.GetAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/coupon/validate?code={IntegrationTestIds.ExpiredCouponCode}&subtotal=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadJsonAsync<CouponValidationResponse>();
        body!.IsValid.Should().BeFalse();
        body.ErrorMessage.Should().Be("Expired coupon.");
    }

    [Fact]
    public async Task Validate_WithoutAuth_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/coupon/validate?code={IntegrationTestIds.ValidCouponCode}&subtotal=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
