using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Services.Common;

public sealed class PaginatedPageLoadService
{
    private readonly IJSRuntime _js;
    private bool _waitAfterRender;
    private bool _waitForImages = true;

    public PaginatedPageLoadService(IJSRuntime js) => _js = js;

    public bool IsLoading { get; private set; }

    public async Task NavigateAsync(Func<Task> loadPageAsync, bool waitForImages = true)
    {
        IsLoading = true;
        _waitForImages = waitForImages;
        await _js.InvokeVoidAsync("foodDelivery.scrollToTop");
        await loadPageAsync();
        _waitAfterRender = true;
    }

    public async Task TryCompleteAfterRenderAsync(ElementReference contentRoot, Func<Task> requestRenderAsync)
    {
        if (!_waitAfterRender)
            return;

        _waitAfterRender = false;
        try
        {
            if (_waitForImages)
                await _js.InvokeVoidAsync("foodDelivery.waitForImages", contentRoot);

            await _js.InvokeVoidAsync("foodDelivery.scrollToTop");
        }
        catch (JSException)
        {
            await _js.InvokeVoidAsync("foodDelivery.scrollToTop");
        }
        finally
        {
            IsLoading = false;
            await requestRenderAsync();
        }
    }
}
