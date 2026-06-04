using System.Net.Http.Json;
using System.Text.Json;
using FoodDelivery.Web.Models.Common;

namespace FoodDelivery.Web.Helpers;

internal static class HttpResultExtensions
{
    public static Task<Result<T>> PostForResultAsync<T>(this HttpClient http, string url, object body) =>
        SendForResultAsync<T>(http.PostAsJsonAsync(url, body));

    public static Task<Result> PutForResultAsync(this HttpClient http, string url, object body) =>
        SendForResultAsync(http.PutAsJsonAsync(url, body));

    public static Task<Result> DeleteForResultAsync(this HttpClient http, string url) =>
        SendForResultAsync(http.DeleteAsync(url));

    private static async Task<Result<T>> SendForResultAsync<T>(Task<HttpResponseMessage> send)
    {
        var response = await send;
        return response.IsSuccessStatusCode
            ? Result<T>.Ok((await response.Content.ReadFromJsonAsync<T>())!)
            : Result<T>.Fail(await ReadErrorAsync(response));
    }

    private static async Task<Result> SendForResultAsync(Task<HttpResponseMessage> send)
    {
        var response = await send;
        return response.IsSuccessStatusCode
            ? Result.Ok()
            : Result.Fail(await ReadErrorAsync(response));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw)) return $"Request failed ({(int)response.StatusCode}).";
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("detail", out var detail)) return detail.GetString()!;
            if (doc.RootElement.TryGetProperty("title", out var title)) return title.GetString()!;
        }
        catch (JsonException) { }
        return raw;
    }
}
