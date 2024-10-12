using NDiscoPlus.PhilipsHue.BridgeDiscovery.Models;
using System.Diagnostics;
using Zeroconf;

namespace NDiscoPlus.PhilipsHue.BridgeDiscovery;


public static partial class HueBridgeDiscovery
{
    /// <summary>
    /// Search bridges using Multicast.
    /// </summary>
    /// <param name="OnBridgeFound">
    /// Return any discovered bridges early through a callback. This callback may come from a different thread.
    /// </param>
    public static async Task<DiscoveredBridge[]> Multicast(TimeSpan scanTime, Action<DiscoveredBridge>? OnBridgeFound = null)
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

        Action<IZeroconfHost>? resolveCallback = null;
        if (OnBridgeFound is not null)
            resolveCallback = host => OnBridgeFound(ConvertToBridge(host));

        IReadOnlyList<IZeroconfHost> result = await ZeroconfResolver.ResolveAsync("_hue._tcp.local.", scanTime: scanTime, callback: resolveCallback);
        return result.Select(static r => ConvertToBridge(r)).ToArray();
    }

    /// <inheritdoc cref="Multicast(TimeSpan, Action{DiscoveredBridge}?)"/>
    public static Task<DiscoveredBridge[]> Multicast(int scanTimeMs = 10_000, Action<DiscoveredBridge>? OnBridgeFound = null)
        => Multicast(TimeSpan.FromMilliseconds(scanTimeMs), OnBridgeFound: OnBridgeFound);
}
