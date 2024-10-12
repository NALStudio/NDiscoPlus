using NDiscoPlus.PhilipsHue.Api.Constants;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using NDiscoPlus.PhilipsHue.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Authentication;
public class HueAuthentication : IDisposable
{
    private readonly EndpointsV1 endpoints;
    private readonly HttpClient http;

    public HueAuthentication(string bridgeIp)
    {
        endpoints = new(bridgeIp);
        http = HueHttpClientProvider.CreateHttp(endpoints.BridgeAddress);
    }

    public async Task<HueCredentials> Authenticate(string appName, string instanceName)
    {
        // Authentication behaviour uses v1 api which means we must write this method completely from scratch

        ThrowIfNameInvalid(appName);
        ThrowIfNameInvalid(instanceName);

        JsonContent content = JsonContent.Create(
            new AuthenticationRequest
            {
                DeviceType = $"{appName}#{instanceName}"
            }
        );

        HttpResponseMessage response = await http.PostAsync(endpoints.Authentication, content);

        // Hue authentication API should return 200 OK on requests with errors
        if (!response.IsSuccessStatusCode)
            throw new HueAuthenticationException($"Request failed: {response.StatusCode}");

        AuthenticationResponse[]? responseContent = await response.Content.ReadFromJsonAsync<AuthenticationResponse[]>();
        AuthenticationResponse? authResponse = responseContent?.Single();

        if (authResponse?.Error is AuthenticationErrorResponse error)
        {
            if (error.Type == 101)
                throw new HueLinkButtonNotPressedException(error.Description);
            else
                throw new HueAuthenticationException($"{error.Type}: {error.Description}");
        }

        if (authResponse?.Success is not AuthenticationSuccessResponse success)
            throw new HueAuthenticationException("Could not deserialize response.");

        if (success.ClientKey is null)
            throw new HueAuthenticationException("No client key returned by bridge.");

        return new HueCredentials(
            AppKey: success.Username,
            ClientKey: success.ClientKey
        );
    }

    private static void ThrowIfNameInvalid(string name, [CallerArgumentExpression(nameof(name))] string? paramName = null)
    {
        if (name.Contains('#'))
            throw new ArgumentException("Invalid character in name: '#'", paramName);

        const int maxLen = 19; // application name can be 20, device name only 19. I'll just limit all to 19 for simplicity
        if (name.Length > maxLen)
            throw new ArgumentException($"Name too long. Maximum character length: {maxLen}.", paramName);
    }

    public void Dispose()
    {
        http.Dispose();
        GC.SuppressFinalize(this);
    }
}
