using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public class UsersApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Block_Unauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsync($"api/users/{IntegrationTestIds.CustomerId}/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Block_AsCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PutAsync($"api/users/{IntegrationTestIds.OwnerId}/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Block_NonExistentUser_ReturnsNotFound()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PutAsync("api/users/99999/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Block_OwnerBlocksAnotherOwner_ReturnsBadRequest()
    {
        var otherOwnerId = await RegisterAsync("Owner");
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PutAsync($"api/users/{otherOwnerId}/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadBodyAsync()).Should().Contain("customers");
    }

    [Fact]
    public async Task Block_OwnerBlocksCustomerWhoNeverOrdered_ReturnsBadRequest()
    {
        var customerId = await RegisterAsync("Customer");
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PutAsync($"api/users/{customerId}/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadBodyAsync()).Should().Contain("ordered");
    }

    [Fact]
    public async Task Block_OwnerBlocksCustomerWhoOrdered_ReturnsNoContent()
    {
        var customerId = await RegisterAsync("Customer");
        await PlaceOrderAsync(customerId);

        var ownerClient = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);
        var response = await ownerClient.PutAsync($"api/users/{customerId}/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Unblock_AsCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PutAsync($"api/users/{IntegrationTestIds.OwnerBlockedCustomerId}/unblock", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Unblock_CustomerNotBlocked_ReturnsNotFound()
    {
        var customerId = await RegisterAsync("Customer");
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PutAsync($"api/users/{customerId}/unblock", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BlockThenUnblock_Customer_RoundTrips()
    {
        var customerId = await RegisterAsync("Customer");
        await PlaceOrderAsync(customerId);
        var ownerClient = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var blockResponse = await ownerClient.PutAsync($"api/users/{customerId}/block", null);
        blockResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var unblockResponse = await ownerClient.PutAsync($"api/users/{customerId}/unblock", null);
        unblockResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task PlaceOrderAsync(int customerId)
    {
        var client = factory.CreateClient().AsUser(factory, customerId);
        var response = await client.PostAsJsonAsync("api/orders", new PlaceOrderRequest(
            IntegrationTestIds.RestaurantId,
            [new OrderItemRequest(IntegrationTestIds.AvailableMealId, 1)],
            Tip: 0m,
            CouponCode: null));
        response.EnsureSuccessStatusCode();
    }

    private async Task<int> RegisterAsync(string role)
    {
        var client = factory.CreateClient();
        var email = $"u_{Guid.NewGuid():N}@test.com";
        var resp = await client.PostAsJsonAsync("api/auth/register", new
        {
            Name = "Test User",
            Email = email,
            Password = "Secret1!",
            Role = role
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }
}
