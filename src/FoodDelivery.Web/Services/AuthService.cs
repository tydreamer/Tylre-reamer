using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public record LoginResponse(string Token, string Name, string Email, string Role);

public class AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
{
    public async Task<string?> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        await js.InvokeVoidAsync("localStorage.setItem", "jwt", result!.Token);
        ((JwtAuthStateProvider)authStateProvider).NotifyUserAuthentication(result.Token);
        return result.Role;
    }

    public async Task<string?> RegisterAsync(string name, string email, string password, string role)
    {
        var response = await http.PostAsJsonAsync("api/auth/register", new { Name = name, Email = email, Password = password, Role = role });
        if (response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsStringAsync();
    }

    public async Task LogoutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", "jwt");
        ((JwtAuthStateProvider)authStateProvider).NotifyUserLoggedOut();
    }
}
