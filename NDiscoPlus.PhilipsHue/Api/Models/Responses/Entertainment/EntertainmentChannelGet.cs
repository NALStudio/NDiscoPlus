using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class EntertainmentChannelGet
{
    internal EntertainmentChannelGet(byte channelId, HuePosition position, ImmutableArray<EntertainmentChannelLocations> locations)
    {
        ChannelId = channelId;
        Position = position;
        Locations = locations;
    }

    public byte ChannelId { get; }
    public HuePosition Position { get; }
    public ImmutableArray<EntertainmentChannelLocations> Locations { get; }
}
