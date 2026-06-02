namespace FoodDelivery.Web.Models.Common;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
