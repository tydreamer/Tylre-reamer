using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public class AuthExpiredHandler(IJSRuntime js, NavigationManager nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            request.RequestUri?.AbsolutePath.Contains("/api/auth/") != true)
        {
            await js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            nav.NavigateTo("/sign-in", forceLoad: true);
        }

        return response;
    }
}
