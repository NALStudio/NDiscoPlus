using NDiscoPlus.PhilipsHue.Api.Constants;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Authentication;
public class HueAuthentication
{
    private readonly EndpointsV1 endpoints;
    private readonly HttpClient http;

    public HueAuthentication(string bridgeIp)
    {
        endpoints = new(bridgeIp);
        http = new();
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

        AuthenticationResponse? responseContent = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        if (responseContent?.Error is AuthenticationErrorResponse error)
        {
            if (error.Type == 101)
                throw new HueLinkButtonNotPressedException(error.Description);
            else
                throw new HueAuthenticationException($"{error.Type}: {error.Description}");
        }

        if (responseContent?.Success is not AuthenticationSuccessResponse success)
            throw new HueAuthenticationException("Could not deserialize response.");

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
}
