namespace FoodDelivery.Web.Components.Browse;

public static class PaginationPageEntries
{
    public static IReadOnlyList<int?> Build(int currentPage, int totalPages)
    {
        if (totalPages <= 0)
            return [];

        if (totalPages <= 3)
            return Enumerable.Range(1, totalPages).Select(p => (int?)p).ToList();

        if (currentPage <= 3)
            return [1, 2, 3, null, totalPages];

        if (currentPage > totalPages - 3)
            return [1, null, totalPages - 2, totalPages - 1, totalPages];

        return [1, null, currentPage, null, totalPages];
    }
}
