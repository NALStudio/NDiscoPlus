using NDiscoPlus.PhilipsHue.BridgeDiscovery.Models;
using System.Diagnostics;
using Zeroconf;

namespace NDiscoPlus.PhilipsHue.BridgeDiscovery;


public static class HueBridgeDiscovery
{
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
}
