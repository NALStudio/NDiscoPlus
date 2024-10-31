using Microsoft.JSInterop;

namespace NDiscoPlus.Code.JavaScriptInterop;

public sealed class WindowSizeProvider : IAsyncDisposable
{
    private readonly record struct WindowSize(int Width, int Height);

    private WindowSize windowSize;
    public int Width => windowSize.Width;
    public int Height => windowSize.Height;

    public double AspectRatio => Width / (double)Height;
    public double InverseAspectRatio => Height / (double)Width;

    private record class JSBridge(WindowSizeProvider Parent)
    {
        [JSInvokable]
        public void OnWindowResized(WindowSize size) => Parent.OnWindowResizeInternal(size);
    }

    private readonly IJSObjectReference _module;
    private readonly DotNetObjectReference<JSBridge> _bridge;

    private WindowSizeProvider(IJSObjectReference module, WindowSize initialWindowSize)
    {
        _module = module;
        windowSize = initialWindowSize;

        JSBridge bridge = new(this);
        _bridge = DotNetObjectReference.Create(bridge);
    }

    public static async Task<WindowSizeProvider> CreateAsync(IJSRuntime js)
    {
        IJSObjectReference module = await js.InvokeAsync<IJSObjectReference>("import", "./js/windowProvider.js");
        WindowSize initialSize = await module.InvokeAsync<WindowSize>("getInnerSize");

        WindowSizeProvider provider = new(module, initialSize);
        await module.InvokeVoidAsync("init", provider._bridge);

        return provider;
    }

    public event Action? OnWindowResize;
    private void OnWindowResizeInternal(WindowSize size)
    {
        windowSize = size;
        OnWindowResize?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        await _module.DisposeAsync();
        _bridge.Dispose();

        GC.SuppressFinalize(this);
    }
}
