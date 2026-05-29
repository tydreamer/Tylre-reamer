using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public class JwtAuthStateProvider(IJSRuntime js, HttpClient http) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", "jwt");
        if (string.IsNullOrWhiteSpace(token))
            return Unauthenticated();

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(token), "jwt")));
    }

    public void NotifyUserAuthentication(string token)
    {
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(token), "jwt"));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLoggedOut()
    {
        http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Unauthenticated()));
    }

    private static AuthenticationState Unauthenticated() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var padded = payload.Length % 4 switch
        {
            2 => payload + "==",
            3 => payload + "=",
            _ => payload
        };
        var jsonBytes = Convert.FromBase64String(padded);
        var kvps = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;
        return kvps.Select(c => new Claim(c.Key, c.Value.ToString()));
    }
}
