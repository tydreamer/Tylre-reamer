using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FoodDelivery.Web.ComponentTests;

internal sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal user = new(new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "test@example.com"),
        new Claim(ClaimTypes.Role, "Customer")
    ],
    authenticationType: "test"));

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(user));
}
