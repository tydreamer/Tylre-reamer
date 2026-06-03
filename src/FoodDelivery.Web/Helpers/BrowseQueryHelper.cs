namespace FoodDelivery.Web.Helpers;

internal static class BrowseQueryHelper
{
    public static string BuildPagedUrl(
        string path,
        int page,
        int pageSize,
        string? search = null,
        IEnumerable<int>? filterIds = null,
        string? filterParameterName = null,
        int? ownerId = null)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (ownerId.HasValue)
            query.Add($"ownerId={ownerId.Value}");

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");

        if (!string.IsNullOrWhiteSpace(filterParameterName) && filterIds is not null)
        {
            foreach (var id in filterIds.Where(i => i > 0).Distinct())
                query.Add($"{filterParameterName}={id}");
        }

        return $"{path}?{string.Join("&", query)}";
    }
}
