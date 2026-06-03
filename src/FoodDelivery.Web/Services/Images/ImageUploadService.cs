using System.Net.Http.Headers;
using System.Net.Http.Json;
using FoodDelivery.Web.Models.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace FoodDelivery.Web.Services.Images;

public class ImageUploadService(HttpClient http)
{
    public const long MaxFileBytes = 5 * 1024 * 1024;

    public async Task<(bool Success, string? ImageUrl, string? Error)> UploadRestaurantImageAsync(
        int restaurantId,
        IBrowserFile file)
    {
        return await UploadAsync($"api/restaurants/{restaurantId}/image", file);
    }

    public async Task<(bool Success, string? ImageUrl, string? Error)> UploadMealImageAsync(
        int restaurantId,
        int mealId,
        IBrowserFile file)
    {
        return await UploadAsync($"api/restaurants/{restaurantId}/meals/{mealId}/image", file);
    }

    public async Task<(bool Success, string? Error)> ClearRestaurantImageAsync(int restaurantId)
    {
        var response = await http.DeleteAsync($"api/restaurants/{restaurantId}/image");
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> ClearMealImageAsync(int restaurantId, int mealId)
    {
        var response = await http.DeleteAsync($"api/restaurants/{restaurantId}/meals/{mealId}/image");
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }

    private async Task<(bool Success, string? ImageUrl, string? Error)> UploadAsync(string url, IBrowserFile file)
    {
        await using var stream = file.OpenReadStream(MaxFileBytes);
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.Name);

        var response = await http.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
            return (true, result?.ImageUrl, null);
        }

        return (false, null, await response.Content.ReadAsStringAsync());
    }
}
