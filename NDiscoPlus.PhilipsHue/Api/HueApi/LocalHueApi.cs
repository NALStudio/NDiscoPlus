using NDiscoPlus.PhilipsHue.Api.Constants;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.HueApi;
public partial class LocalHueApi : IDisposable
{
    private EndpointsV2 Endpoints { get; }

    private readonly HueCredentials credentials;
    private readonly HttpClient http;

    public LocalHueApi(string bridgeIp, HueCredentials credentials)
    {
        this.credentials = credentials;

        Endpoints = new(bridgeIp);

        http = new()
        {
            BaseAddress = new Uri(Endpoints.BaseAddress)
        };
        http.DefaultRequestHeaders.Add("hue-application-key", credentials.AppKey);
    }

    public void Dispose()
    {
        http?.Dispose();
        GC.SuppressFinalize(this);
    }
}
