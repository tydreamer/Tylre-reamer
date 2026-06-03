using System.Net.Http.Json;
using FoodDelivery.API.DTOs.Common;
using FoodDelivery.API.DTOs.Meals;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Browse;

[Collection(nameof(IntegrationTestCollection))]
public sealed class BrowseFilterApiTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMeals_WithMealTypeFilter_ReturnsOnlyMatchingMeals()
    {
        var all = await _client.GetFromJsonAsync<PagedResult<MealBrowseResponse>>(
            "api/meals?page=1&pageSize=20");
        Assert.NotNull(all);
        Assert.Equal(2, all!.TotalCount);

        var mains = await _client.GetFromJsonAsync<PagedResult<MealBrowseResponse>>(
            "api/meals?page=1&pageSize=20&mealTypeIds=1");
        Assert.NotNull(mains);
        Assert.Equal(1, mains!.TotalCount);
        Assert.Equal("Main", mains.Items[0].MealTypeName);

        var desserts = await _client.GetFromJsonAsync<PagedResult<MealBrowseResponse>>(
            "api/meals?page=1&pageSize=20&mealTypeIds=2");
        Assert.NotNull(desserts);
        Assert.Equal(1, desserts!.TotalCount);
        Assert.Equal("Dessert", desserts.Items[0].MealTypeName);
    }
}
