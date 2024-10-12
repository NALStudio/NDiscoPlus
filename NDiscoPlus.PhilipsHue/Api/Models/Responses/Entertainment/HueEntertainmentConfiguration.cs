using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class HueEntertainmentConfiguration
{
    [JsonConstructor]
    internal HueEntertainmentConfiguration(Guid id, HueNameMetadata metadata, string configurationType, HueResourceIdentifier? activeStreamer, ImmutableArray<HueEntertainmentChannel> channels, HueEntertainmentChannelLocations locations)
    {
        Id = id;
        Metadata = metadata;
        ConfigurationType = configurationType;
        ActiveStreamer = activeStreamer;
        Channels = channels;
        Locations = locations;
    }

    public Guid Id { get; }
    public HueNameMetadata Metadata { get; }

    [JsonPropertyName("configuration_type")]
    public string ConfigurationType { get; }

    [JsonPropertyName("active_streamer")]
    public HueResourceIdentifier? ActiveStreamer { get; }

    public ImmutableArray<HueEntertainmentChannel> Channels { get; }
    public HueEntertainmentChannelLocations Locations { get; }

    // TODO: Add missing functionality
}
