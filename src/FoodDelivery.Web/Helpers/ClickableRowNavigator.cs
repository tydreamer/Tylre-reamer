using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Helpers;

public sealed class ClickableRowNavigator(IJSRuntime js)
{
    private const int ClickMoveThresholdPx = 5;

    private int? pendingOrderId;
    private double? pointerDownX;
    private double? pointerDownY;

    public void OnPointerDown(MouseEventArgs e, int orderId)
    {
        pendingOrderId = orderId;
        pointerDownX = e.ClientX;
        pointerDownY = e.ClientY;
    }

    public async Task OnPointerUpAsync(MouseEventArgs e, Action<int> navigate)
    {
        if (pendingOrderId is null || pointerDownX is null || pointerDownY is null)
            return;

        var orderId = pendingOrderId.Value;
        var dx = Math.Abs(e.ClientX - pointerDownX.Value);
        var dy = Math.Abs(e.ClientY - pointerDownY.Value);
        pendingOrderId = null;
        pointerDownX = null;
        pointerDownY = null;

        if (dx > ClickMoveThresholdPx || dy > ClickMoveThresholdPx)
            return;

        if (await js.InvokeAsync<bool>("textSelection.hasTextSelection"))
            return;

        navigate(orderId);
    }
}
