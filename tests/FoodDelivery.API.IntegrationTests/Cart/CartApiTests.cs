using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.API.Constants;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Cart;

[Collection(IntegrationTestCollection.Name)]
public class CartApiTests(CustomWebApplicationFactory factory)
{
    private async Task<HttpClient> CreateCustomerClientAsync()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);
        await client.DeleteAsync("api/cart");
        return client;
    }

    [Fact]
    public async Task Get_EmptyCart_ReturnsEmptyResponse()
    {
        var client = await CreateCustomerClientAsync();

        var response = await client.GetAsync("api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.ReadJsonAsync<CartResponse>();
        cart.Should().NotBeNull();
        cart!.Items.Should().BeEmpty();
        cart.RestaurantId.Should().BeNull();
    }

    [Fact]
    public async Task AddItem_ValidMeal_ReturnsUpdatedCart()
    {
        var client = await CreateCustomerClientAsync();

        var response = await client.PostAsJsonAsync(
            "api/cart/items",
            new AddCartItemRequest(IntegrationTestIds.AvailableMealId, 2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.ReadJsonAsync<CartResponse>();
        cart.Should().NotBeNull();
        cart!.RestaurantId.Should().Be(IntegrationTestIds.RestaurantId);
        cart.Items.Should().ContainSingle(i => i.Meal.Id == IntegrationTestIds.AvailableMealId && i.Quantity == 2);
    }

    [Fact]
    public async Task Get_AfterAdd_ReturnsPersistedCart()
    {
        var client = await CreateCustomerClientAsync();

        await client.PostAsJsonAsync(
            "api/cart/items",
            new AddCartItemRequest(IntegrationTestIds.AvailableMealId, 1));

        var response = await client.GetAsync("api/cart");
        var cart = await response.ReadJsonAsync<CartResponse>();

        cart!.Items.Should().ContainSingle(i => i.Quantity == 1);
    }

    [Fact]
    public async Task Clear_RemovesAllItems()
    {
        var client = await CreateCustomerClientAsync();

        await client.PostAsJsonAsync(
            "api/cart/items",
            new AddCartItemRequest(IntegrationTestIds.AvailableMealId, 1));

        var response = await client.DeleteAsync("api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.ReadJsonAsync<CartResponse>();
        cart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_Unauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_AsOwner_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.GetAsync("api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddItem_DifferentRestaurant_ReturnsBadRequest()
    {
        var client = await CreateCustomerClientAsync();

        await client.PostAsJsonAsync(
            "api/cart/items",
            new AddCartItemRequest(IntegrationTestIds.AvailableMealId, 1));

        var response = await client.PostAsJsonAsync(
            "api/cart/items",
            new AddCartItemRequest(IntegrationTestIds.OtherRestaurantMealId, 1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadBodyAsync()).Should().Be(ValidationMessages.CartRestaurantMismatch);
    }
}
