using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class EntertainmentConfigurationGet
{
    internal EntertainmentConfigurationGet(Guid id, NameMetadata metadata, string configurationType, HueResourceIdentifier? activeStreamer, ImmutableArray<EntertainmentChannelGet> channels)
    {
        Id = id;
        Metadata = metadata;
        ConfigurationType = configurationType;
        ActiveStreamer = activeStreamer;
        Channels = channels;
    }

    public Guid Id { get; }
    public NameMetadata Metadata { get; }

    [JsonPropertyName("configuration_type")]
    public string ConfigurationType { get; }

    [JsonPropertyName("active_streamer")]
    public HueResourceIdentifier? ActiveStreamer { get; }

    public ImmutableArray<EntertainmentChannelGet> Channels { get; }

    // TODO: Add missing functionality
}
