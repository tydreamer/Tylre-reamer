using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services.Auth;

public record LoginResponse(string Token, string Name, string Email, string Role);

public class AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
{
    public async Task<string?> CompleteGoogleLoginAsync(string token, string? role)
    {
        await js.InvokeVoidAsync("localStorage.setItem", "jwt", token);
        ((JwtAuthStateProvider)authStateProvider).NotifyUserAuthentication(token);
        return string.IsNullOrWhiteSpace(role) ? null : role;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        await js.InvokeVoidAsync("localStorage.setItem", "jwt", result!.Token);
        ((JwtAuthStateProvider)authStateProvider).NotifyUserAuthentication(result.Token);
        return result.Role;
    }

    public Task<Result> RegisterAsync(string name, string email, string password, string role) =>
        http.PostForResultAsync("api/auth/register", new { Name = name, Email = email, Password = password, Role = role });

    public async Task LogoutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", "jwt");
        ((JwtAuthStateProvider)authStateProvider).NotifyUserLoggedOut();
    }

    public async Task<(bool Success, string Message, string? ResetUrl)> ForgotPasswordAsync(string email)
    {
        var response = await http.PostAsJsonAsync("api/auth/forgot-password", new { Email = email });
        if (!response.IsSuccessStatusCode)
            return (false, "Unable to process your request. Please try again.", null);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        return (true, result?.Message ?? "If an account exists for this email, you will receive password reset instructions.", result?.ResetUrl);
    }

    public Task<Result> ResetPasswordAsync(string token, string newPassword) =>
        http.PostForResultAsync("api/auth/reset-password", new { Token = token, NewPassword = newPassword });
}

public record ForgotPasswordResponse(string Message, string? ResetUrl);
