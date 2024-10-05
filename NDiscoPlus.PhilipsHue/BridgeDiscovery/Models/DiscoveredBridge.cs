using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.BridgeDiscovery.Models;

public record DiscoveredBridge
{
    [JsonPropertyName("name")]
    public required string? Name { get; init; }

    [JsonPropertyName("internalipaddress")]
    public required string IpAddress { get; init; }

    [JsonPropertyName("id")]
    public required string BridgeId { get; init; }
}