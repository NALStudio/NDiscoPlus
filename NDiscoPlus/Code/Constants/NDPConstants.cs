using SpotifyAPI.Web;
using System.Collections.Immutable;

namespace NDiscoPlus.Code.Constants;

internal static class NDPConstants
{
    public const string SpotifyClientId = "380293db77254cfb91563286874d4dca";
    // We don't store the client secret and use PKCE instead for security reasons.

    public static readonly ImmutableArray<string> SpotifyScope = [Scopes.UserReadPlaybackState];

    public const string SpotifyRedirectUri = "ndiscoplus://spotify-login/";

    public const string HueAppName = "ndiscoplus";
    public const string HueInstanceName =
#if WINDOWS
        "app_windows";
#elif ANDROID
        "app_android";
#elif IOS
        "app_ios";
#elif MACCATALYST
        "app_maccatalyst";
#endif
}
