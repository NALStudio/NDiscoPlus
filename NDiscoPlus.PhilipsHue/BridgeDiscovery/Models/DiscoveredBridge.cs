using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.BridgeDiscovery.Models;

public record DiscoveredBridge
{
    // JsonPropertyNames already added for when we implement discovery endpoint.

    [JsonPropertyName("name")]
    public required string? Name { get; init; }

    [JsonPropertyName("internalipaddress")]
    public required string IpAddress { get; init; }

    [JsonPropertyName("id")] // Nullable because we don't know this during multicast
    public required string? BridgeId { get; init; }
}