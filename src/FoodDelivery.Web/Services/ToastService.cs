namespace FoodDelivery.Web.Services;

public enum ToastKind
{
    Success,
    Error,
    Info
}

public sealed record AppToast(Guid Id, string Message, ToastKind Kind);

public class ToastService
{
    private const int MaxVisibleToasts = 3;

    private readonly List<AppToast> toasts = [];

    public IReadOnlyList<AppToast> Active => toasts;

    public event Action? OnChange;

    public void ShowSuccess(string message, int dismissMs = 5000) =>
        Show(message, ToastKind.Success, dismissMs);

    public void ShowError(string message, int dismissMs = 5000) =>
        Show(message, ToastKind.Error, dismissMs);

    private void Show(string message, ToastKind kind, int dismissMs)
    {
        var toast = new AppToast(Guid.NewGuid(), message, kind);
        toasts.Add(toast);

        while (toasts.Count > MaxVisibleToasts)
            toasts.RemoveAt(0);

        OnChange?.Invoke();

        if (dismissMs > 0)
            _ = DismissAfterAsync(toast.Id, dismissMs);
    }

    private async Task DismissAfterAsync(Guid id, int dismissMs)
    {
        await Task.Delay(dismissMs);
        Dismiss(id);
    }

    public void Dismiss(Guid id)
    {
        if (toasts.RemoveAll(t => t.Id == id) > 0)
            OnChange?.Invoke();
    }
}
