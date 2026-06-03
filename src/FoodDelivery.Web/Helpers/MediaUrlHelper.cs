namespace FoodDelivery.Web.Helpers;

public static class MediaUrlHelper
{
    public static string? Resolve(string? imageUrl, string apiBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return imageUrl;

        var baseUrl = apiBaseUrl.TrimEnd('/');
        return imageUrl.StartsWith('/')
            ? baseUrl + imageUrl
            : baseUrl + "/" + imageUrl;
    }
}
