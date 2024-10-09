using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class HueEntertainmentConfiguration
{
    internal HueEntertainmentConfiguration(Guid id, HueNameMetadata metadata, string configurationType, HueResourceIdentifier? activeStreamer, ImmutableArray<HueEntertainmentChannel> channels)
    {
        Id = id;
        Metadata = metadata;
        ConfigurationType = configurationType;
        ActiveStreamer = activeStreamer;
        Channels = channels;
    }

    public Guid Id { get; }
    public HueNameMetadata Metadata { get; }

    [JsonPropertyName("configuration_type")]
    public string ConfigurationType { get; }

    [JsonPropertyName("active_streamer")]
    public HueResourceIdentifier? ActiveStreamer { get; }

    public ImmutableArray<HueEntertainmentChannel> Channels { get; }
    public ImmutableArray<HueEntertainmentChannelLocations> Locations { get; }

    // TODO: Add missing functionality
}
