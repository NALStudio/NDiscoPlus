using NDiscoPlus.PhilipsHue.BridgeDiscovery.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;

namespace NDiscoPlus.PhilipsHue.BridgeDiscovery;


public static class HueBridgeDiscovery
{
    /* NOT SUPPORTED IN WASM
    /// <summary>
    /// Search bridges using Multicast.
    /// </summary>
    public static async Task<DiscoveredBridge[]> Multicast(TimeSpan scanTime)
    {
        static DiscoveredBridge ConvertToBridge(IZeroconfHost host)
        {
            IReadOnlyDictionary<string, string> properties = host.Services["hue"].Properties.Single();

            string bridgeId = properties["bridgeid"];

            Debug.Assert(host.DisplayName.EndsWith($" - {bridgeId[..^6]}"));
            string name = host.DisplayName[..^9];

            return new()
            {
                Name = name,
                BridgeId = bridgeId,
                IpAddress = host.IPAddresses.Single()
            };
        }

        IReadOnlyList<IZeroconfHost> result = await ZeroconfResolver.ResolveAsync("_hue._tcp local.", scanTime: scanTime);
        return result.Select(static r => ConvertToBridge(r)).ToArray();
    }

    /// <inheritdoc cref="Multicast(TimeSpan)"/>
    public static Task<DiscoveredBridge[]> Multicast(int scanTimeMs = 10_000)
        => Multicast(TimeSpan.FromMilliseconds(scanTimeMs));
    */

    /// <summary>
    /// Fetch all bridges that are currently considered active by the Philips Hue discovery endpoint.
    /// </summary>
    /// <returns>
    /// All currently active bridges or <see langword="null"/> if fetch failed.
    /// </returns>
    /// <remarks>
    /// Rate limit: 1 request every 15 minutes
    /// </remarks>
    public static async Task<DiscoveredBridge[]?> Endpoint(HttpClient httpClient)
    {
        HttpResponseMessage response = await httpClient.GetAsync(Endpoints.Discovery);
        if (!response.IsSuccessStatusCode)
            return null;

        Stream content = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<DiscoveredBridge[]>(content);
    }

    /// <inheritdoc cref="Endpoint(HttpClient)"/>
    public static Task<DiscoveredBridge[]?> Endpoint()
        => Endpoint(new HttpClient());
}
