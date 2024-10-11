using NDiscoPlus.PhilipsHue.BridgeDiscovery.Models;
using System.Diagnostics;
using Zeroconf;

namespace NDiscoPlus.PhilipsHue.BridgeDiscovery;


public static partial class HueBridgeDiscovery
{
    /// <summary>
    /// Search bridges using Multicast.
    /// </summary>
    public static async Task<DiscoveredBridge[]> Multicast(TimeSpan scanTime)
    {
        static DiscoveredBridge ConvertToBridge(IZeroconfHost host)
        {
            return new()
            {
                Name = host.DisplayName,
                BridgeId = null,
                IpAddress = host.IPAddress
            };
        }

        IReadOnlyList<IZeroconfHost> result = await ZeroconfResolver.ResolveAsync("_hue._tcp.local.", scanTime: scanTime);
        return result.Select(static r => ConvertToBridge(r)).ToArray();
    }

    /// <inheritdoc cref="Multicast(TimeSpan)"/>
    public static Task<DiscoveredBridge[]> Multicast(int scanTimeMs = 10_000)
        => Multicast(TimeSpan.FromMilliseconds(scanTimeMs));
}
