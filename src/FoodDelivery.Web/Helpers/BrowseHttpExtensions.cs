using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FoodDelivery.Web.Helpers;

internal static class BrowseHttpExtensions
{
    public static async Task<T?> GetBrowseJsonAsync<T>(this HttpClient http, string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
        request.Headers.Pragma.ParseAdd("no-cache");

        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }
}
