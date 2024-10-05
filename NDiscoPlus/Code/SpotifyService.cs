using NDiscoPlus.Code.Constants;
using NDiscoPlus.Spotify.Players;
using SpotifyAPI.Web;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoPlus.Code;

public class SpotifyService
{
    public SpotifyClient? Client { get; private set; }

    [MemberNotNullWhen(true, nameof(Client))]
    public bool IsLoggedIn => Client is not null;

    public event Action<PKCETokenResponse>? TokenRefreshed;

    private readonly TaskCompletionSource waitForLoginTaskSource = new();
    public Task WaitForLogin() => waitForLoginTaskSource.Task;

    public async Task LoginSpotify(string refreshToken)
    {
        if (IsLoggedIn)
            throw new InvalidOperationException("Already logged in.");

        PKCETokenResponse oauthResp = await new OAuthClient().RequestToken(
            new PKCETokenRefreshRequest(NDPConstants.SpotifyClientId, refreshToken)
        );
        OnTokenRefreshed(oauthResp);

        PKCEAuthenticator authenticator = new(NDPConstants.SpotifyClientId, oauthResp);
        authenticator.TokenRefreshed += OnTokenRefreshed;

        Client = new SpotifyClient(
            SpotifyClientConfig.CreateDefault()
            .WithAuthenticator(authenticator)
        );
        waitForLoginTaskSource.SetResult();
    }

    private void OnTokenRefreshed(object? sender, PKCETokenResponse e) => OnTokenRefreshed(e);
    private void OnTokenRefreshed(PKCETokenResponse e) => TokenRefreshed?.Invoke(e);
}