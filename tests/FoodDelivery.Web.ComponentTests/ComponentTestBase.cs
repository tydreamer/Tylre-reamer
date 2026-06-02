using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDelivery.Web.ComponentTests;

public abstract class ComponentTestBase : BunitContext
{
    protected ComponentTestBase()
    {
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestAuthenticationStateProvider>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
