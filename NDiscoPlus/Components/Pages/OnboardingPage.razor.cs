using Microsoft.Extensions.Logging;
using NDiscoPlus.Code;
using NDiscoPlus.Code.Constants;
using NDiscoPlus.Code.Models;
using SpotifyAPI.Web;
using System.Security.Cryptography;

namespace NDiscoPlus.Components.Pages;

public partial class OnboardingPage
{
    private static class AuthenticationHandling
    {
        private class AuthenticationException : Exception
        {
            public AuthenticationException() : base()
            {
            }
            public AuthenticationException(string? message) : base(message)
            {
            }
            public AuthenticationException(string? message, Exception? innerException) : base(message, innerException)
            {
            }
        }

        private record AuthenticationData(
            Uri RedirectUri,
            string Verifier,
            string State
        );

        public static async Task AuthenticationRequest()
        {
            (string verifier, string challenge) = PKCEUtil.GenerateCodes();

            AuthenticationData authentication = new(
                RedirectUri: new(NDPConstants.SpotifyRedirectUri),
                Verifier: verifier,
                State: RandomNumberGenerator.GetHexString(64)
            );

            LoginRequest loginRequest = new(
                authentication.RedirectUri,
                NDPConstants.SpotifyClientId,
                LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = NDPConstants.SpotifyScope,
                State = authentication.State
            };

            string code = await Authenticate(loginRequest.ToUri(), authentication);
            await LoginUsingAuthenticationCode(code: code, authentication: authentication);
        }


        private static async Task<string> Authenticate(Uri loginRequestUri, AuthenticationData authentication)
        {
            Dictionary<string, string> properties;
#if WINDOWS
            WinUIEx.WebAuthenticatorResult winAuthResult = await WinUIEx.WebAuthenticator.AuthenticateAsync(loginRequestUri, authentication.RedirectUri);
            properties = winAuthResult.Properties;
#else
            WebAuthenticatorResult mauiAuthResult = await WebAuthenticator.AuthenticateAsync(loginRequestUri, authentication.RedirectUri);
            properties = mauiAuthResult.Properties;
#endif

            if (!properties.TryGetValue("state", out string? state))
                throw new AuthenticationException("No state in response.");
            if (state != authentication.State)
                throw new AuthenticationException("Invalid state.");

            if (!properties.TryGetValue("code", out string? code))
                throw new AuthenticationException("No code in response.");
            return code;
        }

        private static async Task LoginUsingAuthenticationCode(string code, AuthenticationData authentication)
        {
            PKCETokenResponse oauthResp = await new OAuthClient().RequestToken(
                new PKCETokenRequest(
                    NDPConstants.SpotifyClientId,
                    code,
                    new Uri(NDPConstants.SpotifyRedirectUri),
                    authentication.Verifier
                )
            );

            StoredSpotifyRefreshToken refreshToken = new(oauthResp.RefreshToken, NDPConstants.SpotifyScope);
            await Settings.Authentication.SetSpotifyRefreshToken(refreshToken);
        }
    }
}