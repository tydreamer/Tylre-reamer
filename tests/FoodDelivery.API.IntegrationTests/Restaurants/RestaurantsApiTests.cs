using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Restaurants;

[Collection(IntegrationTestCollection.Name)]
public class RestaurantsApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task GetAll_NoAuth_ReturnsPagedRestaurants()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/restaurants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.ReadJsonAsync<PagedResult<RestaurantResponse>>();
        page!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ExistingRestaurant_ReturnsRestaurant()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"api/restaurants/{IntegrationTestIds.RestaurantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var restaurant = await response.ReadJsonAsync<RestaurantResponse>();
        restaurant!.Id.Should().Be(IntegrationTestIds.RestaurantId);
    }

    [Fact]
    public async Task GetById_NonExistingRestaurant_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/restaurants/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_AsOwner_ReturnsCreatedWithCorrectOwner()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PostAsJsonAsync("api/restaurants", new
        {
            Name = "New Bistro",
            Description = "A test restaurant",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 40.0,
            Longitude = -73.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var restaurant = await response.ReadJsonAsync<RestaurantResponse>();
        restaurant!.Name.Should().Be("New Bistro");
        restaurant.OwnerId.Should().Be(IntegrationTestIds.OwnerId);
    }

    [Fact]
    public async Task Create_AsCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PostAsJsonAsync("api/restaurants", new
        {
            Name = "Sneaky",
            Description = "desc",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 0.0,
            Longitude = 0.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_InvalidCuisineId_ReturnsBadRequest()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PostAsJsonAsync("api/restaurants", new
        {
            Name = "Bad Cuisine",
            Description = "desc",
            ImageUrl = "",
            CuisineId = 9999,
            Latitude = 0.0,
            Longitude = 0.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Unauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/restaurants", new
        {
            Name = "No Auth",
            Description = "desc",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 0.0,
            Longitude = 0.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsOwner_ReturnsNoContent()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        // Create a fresh restaurant to update so we don't mutate seeded data.
        var created = await client.PostAsJsonAsync("api/restaurants", new
        {
            Name = "To Update",
            Description = "desc",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 0.0,
            Longitude = 0.0
        });
        var newRestaurant = await created.ReadJsonAsync<RestaurantResponse>();

        var response = await client.PutAsJsonAsync($"api/restaurants/{newRestaurant!.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated description",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 0.0,
            Longitude = 0.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_AsCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PutAsJsonAsync($"api/restaurants/{IntegrationTestIds.RestaurantId}", new
        {
            Name = "Stolen",
            Description = "desc",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 0.0,
            Longitude = 0.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsOwner_ReturnsNoContent()
    {
        // Create a restaurant to delete so we don't affect other tests
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var createResp = await client.PostAsJsonAsync("api/restaurants", new
        {
            Name = "To Delete",
            Description = "desc",
            ImageUrl = "",
            CuisineId = 1,
            Latitude = 0.0,
            Longitude = 0.0
        });
        var created = await createResp.ReadJsonAsync<RestaurantResponse>();

        var response = await client.DeleteAsync($"api/restaurants/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
