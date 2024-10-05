using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Shared.Effects.API;

internal class EffectAPI
{
    public EffectConfig Config { get; }

    public IList<EffectChannel> Channels => channelsArr;
    private readonly ImmutableArray<EffectChannel> channelsArr;
    private readonly ImmutableDictionary<Channel, EffectChannel> channelsDic;

    public BackgroundChannel? Background { get; }

    public EffectAPI(EffectConfig config, ImmutableArray<LightRecord> lights)
    {
        KeyValuePair<Channel, EffectChannel>? CreateChannelIfNecessary(Channel type)
        {
            NDPLight[] channelLights = lights.Where(l => l.Channel.HasFlag(type))
                                             .Select(l => l.Light)
                                             .ToArray();
            if (channelLights.Length > 0)
                return new(type, new EffectChannel(type, channelLights));
            else
                return null;
        }

        Config = config;

        List<KeyValuePair<Channel, EffectChannel>> channels = new(capacity: ChannelFlag.FlagValues.Length);
        foreach (Channel c in ChannelFlag.FlagValues)
        {
            KeyValuePair<Channel, EffectChannel>? chnl = CreateChannelIfNecessary(c);
            if (chnl.HasValue)
                channels.Add(chnl.Value);
        }

        channelsArr = channels.Select(c => c.Value).ToImmutableArray();
        channelsDic = channels.ToImmutableDictionary();

        if (channelsDic.TryGetValue(Channel.Background, out EffectChannel? backgroundChannel))
            Background = new(backgroundChannel.Lights);
        else
            Background = null;
    }

    public EffectChannel? GetChannel(Channel type)
    {
        if (channelsDic.TryGetValue(type, out EffectChannel? value))
            return value;
        return null;
    }
    // Not needed, just check for null.
    // public bool TryGetChannel(Channel type, [MaybeNullWhen(false)] out EffectChannel channel)
    //     => channelsDic.TryGetValue(type, out channel);
}