using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FoodDelivery.API.DTOs.Meals;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Meals;

[Collection(IntegrationTestCollection.Name)]
public class MealsApiTests(CustomWebApplicationFactory factory)
{
    private string MealsUrl(int restaurantId) => $"api/restaurants/{restaurantId}/meals";

    [Fact]
    public async Task GetAll_NoAuth_ReturnsRestaurantMeals()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(MealsUrl(IntegrationTestIds.RestaurantId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var meals = await response.ReadJsonAsync<List<MealResponse>>();
        meals.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_ExistingMeal_ReturnsMeal()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"{MealsUrl(IntegrationTestIds.RestaurantId)}/{IntegrationTestIds.AvailableMealId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var meal = await response.ReadJsonAsync<MealResponse>();
        meal!.Id.Should().Be(IntegrationTestIds.AvailableMealId);
    }

    [Fact]
    public async Task GetById_NonExistentMeal_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"{MealsUrl(IntegrationTestIds.RestaurantId)}/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_MealFromDifferentRestaurant_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        // Meal exists, but not under OtherRestaurantId.
        var response = await client.GetAsync(
            $"{MealsUrl(IntegrationTestIds.OtherRestaurantId)}/{IntegrationTestIds.AvailableMealId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_AsOwner_ReturnsCreated()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PostAsJsonAsync(
            MealsUrl(IntegrationTestIds.RestaurantId),
            new CreateMealRequest("New Meal", "Desc", "", 9.99m, 1));

        var body = await response.ReadBodyAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
    }

    [Fact]
    public async Task Create_AsCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);

        var response = await client.PostAsJsonAsync(
            MealsUrl(IntegrationTestIds.RestaurantId),
            new CreateMealRequest("Sneaky", "Desc", "", 5m, 1));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_Unauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            MealsUrl(IntegrationTestIds.RestaurantId),
            new CreateMealRequest("No Auth", "Desc", "", 5m, 1));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_InvalidMealType_ReturnsBadRequest()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PostAsJsonAsync(
            MealsUrl(IntegrationTestIds.RestaurantId),
            new CreateMealRequest("Bad Type", "Desc", "", 5m, 9999));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_NonExistentRestaurant_ReturnsNotFound()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PostAsJsonAsync(
            MealsUrl(99999),
            new CreateMealRequest("Orphan", "Desc", "", 5m, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_RestaurantOwnedByAnotherOwner_ReturnsForbidden()
    {
        var otherOwnerId = await RegisterAsync("Owner");
        var client = factory.CreateClient().AsUser(factory, otherOwnerId);

        var response = await client.PostAsJsonAsync(
            MealsUrl(IntegrationTestIds.RestaurantId), // owned by seeded OwnerId
            new CreateMealRequest("Intruder", "Desc", "", 5m, 1));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_AsOwner_ReturnsNoContent()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);
        var mealId = await CreateMealAsync(client);

        var response = await client.PutAsJsonAsync(
            $"{MealsUrl(IntegrationTestIds.RestaurantId)}/{mealId}",
            new UpdateMealRequest("Updated Name", "Updated Desc", "", 12.50m, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_NonExistentMeal_ReturnsNotFound()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PutAsJsonAsync(
            $"{MealsUrl(IntegrationTestIds.RestaurantId)}/99999",
            new UpdateMealRequest("X", "Y", "", 1m, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsOwner_ReturnsNoContent()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);
        var mealId = await CreateMealAsync(client);

        var response = await client.DeleteAsync(
            $"{MealsUrl(IntegrationTestIds.RestaurantId)}/{mealId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistentMeal_ReturnsNotFound()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.DeleteAsync(
            $"{MealsUrl(IntegrationTestIds.RestaurantId)}/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<int> CreateMealAsync(HttpClient ownerClient)
    {
        var response = await ownerClient.PostAsJsonAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/meals",
            new CreateMealRequest("Temp Meal", "Desc", "", 8m, 1));
        response.EnsureSuccessStatusCode();
        var meal = await response.Content.ReadFromJsonAsync<MealResponse>();
        return meal!.Id;
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
