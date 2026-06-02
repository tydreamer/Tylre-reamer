using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.API.Constants;
using FoodDelivery.API.IntegrationTests.Infrastructure;
using FoodDelivery.API.Models;

namespace FoodDelivery.API.IntegrationTests.Orders;

[Collection(IntegrationTestCollection.Name)]
public class OrdersApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task PlaceOrder_ValidRequest_ReturnsCreated()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.AvailableMealId, 2)],
            Tip: 2m,
            CouponCode: null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.ReadJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.TotalPrice.Should().Be(22m); // 10 * 2 + 2 tip
        order.RestaurantName.Should().Be("Test Bistro");
    }

    [Fact]
    public async Task PlaceOrder_BlockedByOwner_ReturnsBadRequestWithPlainMessage()
    {
        var client = factory.CreateClient()
            .AsUser(factory, IntegrationTestIds.OwnerBlockedCustomerId);

        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.AvailableMealId, 1)],
            Tip: 0m,
            CouponCode: null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadBodyAsync()).Should().Be(ValidationMessages.CustomerBlockedFromRestaurant);
    }

    [Fact]
    public async Task PlaceOrder_GloballyBlockedCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient()
            .AsUser(factory, IntegrationTestIds.GloballyBlockedCustomerId);

        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.AvailableMealId, 1)],
            Tip: 0m,
            CouponCode: null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PlaceOrder_UnavailableMeal_ReturnsBadRequest()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.UnavailableMealId, 1)],
            Tip: 0m,
            CouponCode: null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadBodyAsync()).Should().Be("One or more meals are invalid or unavailable.");
    }

    [Fact]
    public async Task PlaceOrder_ExpiredCoupon_ReturnsPlainErrorMessage()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.AvailableMealId, 1)],
            Tip: 0m,
            CouponCode: IntegrationTestIds.ExpiredCouponCode));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadBodyAsync()).Should().Be("Expired coupon.");
    }

    [Fact]
    public async Task PlaceOrder_ValidCoupon_AppliesDiscount()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.AvailableMealId, 2)],
            Tip: 0m,
            CouponCode: IntegrationTestIds.ValidCouponCode));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.ReadJsonAsync<OrderResponse>();
        order!.TotalPrice.Should().Be(18m); // 20 subtotal - 10% (2) discount
    }

    [Fact]
    public async Task GetOrders_SortByTotalDescending_ReturnsHighestFirst()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.GetAsync("api/orders?sortBy=total&sortDesc=true&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.ReadJsonAsync<PagedResult<OrderResponse>>();
        page.Should().NotBeNull();
        page!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        page.Items[0].TotalPrice.Should().BeGreaterThanOrEqualTo(page.Items[1].TotalPrice);
    }

    [Fact]
    public async Task GetOrders_AsOwner_ReturnsRestaurantOrdersOnly()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.GetAsync("api/orders?pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.ReadJsonAsync<PagedResult<OrderResponse>>();
        page!.Items.Should().NotBeEmpty();
        page.Items.Should().OnlyContain(o => o.RestaurantId == IntegrationTestIds.RestaurantId);
    }
}
