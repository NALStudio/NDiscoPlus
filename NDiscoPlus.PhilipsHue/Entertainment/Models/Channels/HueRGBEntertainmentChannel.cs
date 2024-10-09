using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment.Models.Channels;

public readonly struct HueRGBEntertainmentChannel : IHueEntertainmentChannel
{
    public byte Id { get; }

    public required ushort R { get; init; }
    public required ushort G { get; init; }
    public required ushort B { get; init; }

    public HueRGBEntertainmentChannel(byte id)
    {
        Id = id;
    }

    ushort IHueEntertainmentChannel.ColorChannel1 => R;
    ushort IHueEntertainmentChannel.ColorChannel2 => G;
    ushort IHueEntertainmentChannel.ColorChannel3 => B;
}
