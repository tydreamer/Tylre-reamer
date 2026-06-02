using System.Net.Http.Headers;
using System.Net.Http.Json;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDelivery.API.IntegrationTests.Infrastructure;

public static class HttpClientExtensions
{
    public static HttpClient AsUser(this HttpClient client, CustomWebApplicationFactory factory, int userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodDelivery.API.Data.AppDbContext>();
        var user = db.Users.Find(userId)
            ?? throw new InvalidOperationException($"Test user {userId} was not seeded.");

        var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
        var token = jwt.GenerateToken(user);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<string> ReadBodyAsync(this HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();

    public static async Task<T?> ReadJsonAsync<T>(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>();
}
