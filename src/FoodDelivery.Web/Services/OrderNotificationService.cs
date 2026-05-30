using FoodDelivery.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public class OrderNotificationService(IJSRuntime js, IConfiguration config) : IAsyncDisposable
{
    private HubConnection? _connection;

    public event Action<OrderStatusNotification>? OnOrderStatusChanged;

    public async Task StartAsync()
    {
        if (_connection is not null)
            return;

        var token = await js.InvokeAsync<string?>("localStorage.getItem", "jwt");
        if (string.IsNullOrWhiteSpace(token))
            return;

        var hubUrl = $"{config["ApiBaseUrl"]!.TrimEnd('/')}/hubs/orders";
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options => options.AccessTokenProvider = () => Task.FromResult(token)!)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OrderStatusNotification>("OrderStatusChanged", notification =>
        {
            OnOrderStatusChanged?.Invoke(notification);
        });

        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
            return;

        await _connection.DisposeAsync();
        _connection = null;
    }
}
