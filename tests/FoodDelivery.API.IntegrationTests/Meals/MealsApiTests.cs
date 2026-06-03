using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.API.DTOs.Meals;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Meals;

[Collection(IntegrationTestCollection.Name)]
public class MealsApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Create_AsOwner_ReturnsCreated()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);

        var response = await client.PostAsJsonAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/meals",
            new CreateMealRequest("New Meal", "Desc", "", 9.99m, 1));

        var body = await response.ReadBodyAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
    }
}
