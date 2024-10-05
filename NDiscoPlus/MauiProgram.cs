using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NDiscoPlus.Code;
using NDiscoPlus.Code.JavaScriptInterop;

namespace NDiscoPlus;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddMudServices(config =>
        {
            // Configure Snackbar
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            config.SnackbarConfiguration.ShowTransitionDuration = 250;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
            config.SnackbarConfiguration.HideTransitionDuration = 2000;
        });

        builder.Services.AddSingleton<SpotifyService>();

        return builder.Build();
    }
}
