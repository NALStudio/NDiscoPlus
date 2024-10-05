using NDiscoPlus.Shared.Helpers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;

[Flags]
public enum Channel
{
    None = 0,

    // Channel that come after override previous channels 
    Background = 1 << 0,
    Default = 1 << 1,
    Flash = 1 << 2,
    Strobe = 1 << 3,

    // int with all bits set
    All = int.MaxValue
}

public static class ChannelFlag
{
    public static readonly ImmutableArray<string> Names = ImmutableCollectionsMarshal.AsImmutableArray(Enum.GetNames<Channel>());

    public static readonly ImmutableArray<Channel> AllValues = ImmutableCollectionsMarshal.AsImmutableArray(Enum.GetValues<Channel>());
    public static readonly ImmutableArray<Channel> FlagValues = AllValues.Where(x => GetFlagsCount(x) == 1).ToImmutableArray();

    public static readonly ImmutableArray<(string Name, Channel Value)> Items = Names.ZipStrict(AllValues).ToImmutableArray();
    public static readonly ImmutableArray<(string Name, Channel Value)> FlagItems = Items.Where(x => GetFlagsCount(x.Value) == 1).ToImmutableArray();

    public static int GetFlagsCount(Channel channel)
        => int.PopCount((int)channel); // return the amount of set bits in flag
}