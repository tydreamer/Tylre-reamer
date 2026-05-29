using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public record LoginResponse(string Token);

public class AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
{
    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        await js.InvokeVoidAsync("localStorage.setItem", "jwt", result!.Token);
        ((JwtAuthStateProvider)authStateProvider).NotifyUserAuthentication(result.Token);
        return true;
    }

    public async Task<bool> RegisterAsync(string email, string password, string role)
    {
        var response = await http.PostAsJsonAsync("api/auth/register", new { Email = email, Password = password, Role = role });
        return response.IsSuccessStatusCode;
    }

    public async Task LogoutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", "jwt");
        ((JwtAuthStateProvider)authStateProvider).NotifyUserLoggedOut();
    }
}
