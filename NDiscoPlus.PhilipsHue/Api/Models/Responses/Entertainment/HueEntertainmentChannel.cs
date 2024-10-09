using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class HueEntertainmentChannel
{
    internal HueEntertainmentChannel(byte channelId, HuePosition position)
    {
        ChannelId = channelId;
        Position = position;
    }

    public byte ChannelId { get; }
    public HuePosition Position { get; }
    public ImmutableArray<HueSegmentReference> Members { get; }
}
