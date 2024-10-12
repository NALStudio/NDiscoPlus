using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class HueEntertainmentChannel
{
    [JsonConstructor]
    internal HueEntertainmentChannel(byte channelId, HuePosition position, ImmutableArray<HueSegmentReference> members)
    {
        ChannelId = channelId;
        Position = position;
        Members = members;
    }

    [JsonPropertyName("channel_id")]
    public byte ChannelId { get; }
    public HuePosition Position { get; }
    public ImmutableArray<HueSegmentReference> Members { get; }
}
