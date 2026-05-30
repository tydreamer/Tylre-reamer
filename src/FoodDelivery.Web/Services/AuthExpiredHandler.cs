using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public class AuthExpiredHandler(
    IJSRuntime js,
    AuthenticationStateProvider authStateProvider,
    NavigationManager nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            request.RequestUri?.AbsolutePath.Contains("/api/auth/") != true)
        {
            await js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            ((JwtAuthStateProvider)authStateProvider).NotifyUserLoggedOut();
            nav.NavigateTo("/sign-in");
        }

        return response;
    }
}
