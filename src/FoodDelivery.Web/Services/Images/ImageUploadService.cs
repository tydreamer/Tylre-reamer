using System.Net.Http.Headers;
using System.Net.Http.Json;
using FoodDelivery.Web.Models.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace FoodDelivery.Web.Services.Images;

public class ImageUploadService(HttpClient http)
{
    public const long MaxFileBytes = 5 * 1024 * 1024;

    public Task<Result<string>> UploadRestaurantImageAsync(int restaurantId, IBrowserFile file) =>
        UploadAsync($"api/restaurants/{restaurantId}/image", file);

    public Task<Result<string>> UploadMealImageAsync(int restaurantId, int mealId, IBrowserFile file) =>
        UploadAsync($"api/restaurants/{restaurantId}/meals/{mealId}/image", file);

    public Task<Result> ClearRestaurantImageAsync(int restaurantId) =>
        http.DeleteForResultAsync($"api/restaurants/{restaurantId}/image");

    public Task<Result> ClearMealImageAsync(int restaurantId, int mealId) =>
        http.DeleteForResultAsync($"api/restaurants/{restaurantId}/meals/{mealId}/image");

    private async Task<Result<string>> UploadAsync(string url, IBrowserFile file)
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
            return Result<string>.Ok(result?.ImageUrl ?? string.Empty);
        }

        return Result<string>.Fail(await HttpResultExtensions.ReadErrorAsync(response));
    }
}
