using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment.Models.Channels;

// TODO: Change to ref struct once interfaces are supported with them
public readonly struct HueXYEntertainmentChannel : IHueEntertainmentChannel
{
    public byte Id { get; }

    public required ushort X { get; init; }
    public required ushort Y { get; init; }
    public required ushort Brightness { get; init; }

    public HueXYEntertainmentChannel(byte id)
    {
        Id = id;
    }

    ushort IHueEntertainmentChannel.ColorChannel1 => X;
    ushort IHueEntertainmentChannel.ColorChannel2 => Y;
    ushort IHueEntertainmentChannel.ColorChannel3 => Brightness;
}
