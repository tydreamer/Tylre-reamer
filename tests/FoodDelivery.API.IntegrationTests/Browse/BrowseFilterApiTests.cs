using System.Net.Http.Json;
using FoodDelivery.API.DTOs.Common;
using FoodDelivery.API.DTOs.Meals;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Browse;

[Collection(IntegrationTestCollection.Name)]
public sealed class BrowseFilterApiTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMeals_WithMealTypeFilter_ReturnsOnlyMatchingMeals()
    {
        var all = await _client.GetFromJsonAsync<PagedResult<MealBrowseResponse>>(
            "api/meals?page=1&pageSize=100");
        Assert.NotNull(all);
        Assert.True(all!.TotalCount >= 2);

        var mains = await _client.GetFromJsonAsync<PagedResult<MealBrowseResponse>>(
            "api/meals?page=1&pageSize=100&mealTypeIds=1");
        Assert.NotNull(mains);
        Assert.True(mains!.TotalCount >= 1);
        Assert.All(mains.Items, m => Assert.Equal("Main", m.MealTypeName));

        var desserts = await _client.GetFromJsonAsync<PagedResult<MealBrowseResponse>>(
            "api/meals?page=1&pageSize=100&mealTypeIds=2");
        Assert.NotNull(desserts);
        Assert.True(desserts!.TotalCount >= 1);
        Assert.All(desserts.Items, m => Assert.Equal("Dessert", m.MealTypeName));
    }
}
