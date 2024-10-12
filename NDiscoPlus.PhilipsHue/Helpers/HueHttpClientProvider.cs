using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Helpers;
internal static class HueHttpClientProvider
{
    public static HttpClient CreateHttp(string bridgeAddress)
    {
        HttpClientHandler handler = new()
        {
            // !! DANGEROUS !!
            // Q42.HueApi uses this as well and I couldn't find any other solutions to this problem
            // as without accepting all SSL certificates, we can't connect to the bridge.
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(bridgeAddress)
        };
    }
}
