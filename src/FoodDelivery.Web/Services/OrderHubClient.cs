using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services;

public class OrderHubClient(IJSRuntime js, IConfiguration config) : IAsyncDisposable
{
    private HubConnection? _connection;
    private int? _orderId;

    public async Task ConnectAsync(int orderId, Func<Task> onStatusUpdated)
    {
        await DisposeAsync();

        var token = await js.InvokeAsync<string?>("localStorage.getItem", "jwt");
        if (string.IsNullOrWhiteSpace(token))
            return;

        var hubUrl = $"{config["ApiBaseUrl"]!.TrimEnd('/')}/hubs/orders";
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options => options.AccessTokenProvider = () => Task.FromResult(token)!)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("OrderStatusUpdated", async _ =>
        {
            await onStatusUpdated();
        });

        await _connection.StartAsync();
        _orderId = orderId;
        await _connection.InvokeAsync("JoinOrderGroup", orderId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
            return;

        if (_orderId is not null && _connection.State == HubConnectionState.Connected)
        {
            try
            {
                await _connection.InvokeAsync("LeaveOrderGroup", _orderId.Value);
            }
            catch
            {
            }
        }

        await _connection.DisposeAsync();
        _connection = null;
        _orderId = null;
    }
}
