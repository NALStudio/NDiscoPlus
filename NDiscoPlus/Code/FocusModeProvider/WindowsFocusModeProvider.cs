#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Diagnostics.CodeAnalysis;
using WinRT.Interop;

namespace NDiscoPlus.Code.FocusModeProvider;

/// <summary>
/// Windows Focus Mode makes the app go fullscreen.
/// </summary>
internal class WindowsFocusModeProvider : IFocusModeProvider
{
    private readonly OverlappedPresenter? presenter;
    public WindowsFocusModeProvider(Window window)
    {
        _ = TryGetOverlappedPresenter(window, out presenter);
    }

    public bool IsFocusModeEnabled
    {
        get
        {
            if (presenter is null)
                return false;

            // Focus mode is enabled if title bar is hidden
            return !presenter.HasTitleBar;
        }
    }

    public void DisableFocusMode()
    {
        if (presenter is null)
            return;

        presenter.SetBorderAndTitleBar(hasBorder: true, hasTitleBar: true);
        presenter.Restore();
    }

    public void EnableFocusMode()
    {
        if (presenter is null)
            return;

        presenter.SetBorderAndTitleBar(hasBorder: false, hasTitleBar: false);
        presenter.Maximize();
    }

    // https://blog.verslu.is/maui/full-screen-disable-minimize-maximize-for-net-maui-windows-apps/
    private static bool TryGetOverlappedPresenter(Window window, [MaybeNullWhen(false)] out OverlappedPresenter presenter)
    {
        MauiWinUIWindow? winuiWindow = window.Handler.PlatformView as MauiWinUIWindow;
        if (winuiWindow is not null)
        {
            nint handle = WindowNative.GetWindowHandle(winuiWindow);
            WindowId id = Win32Interop.GetWindowIdFromWindow(handle);
            AppWindow appWindow = AppWindow.GetFromWindowId(id);

            if (appWindow.Presenter is OverlappedPresenter op)
            {
                presenter = op;
                return true;
            }
        }

        presenter = null;
        return false;
    }
}
#endif