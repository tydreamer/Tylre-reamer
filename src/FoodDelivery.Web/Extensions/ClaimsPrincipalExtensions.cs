using System.Security.Claims;

namespace FoodDelivery.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsOwner(this ClaimsPrincipal user) =>
        user.IsInRole("Owner");

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(id, out var userId) ? userId : null;
    }
}
