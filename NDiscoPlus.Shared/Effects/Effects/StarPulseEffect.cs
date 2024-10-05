using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Effects;

internal class StarPulseEffect : NDPEffect
{
    private static readonly NDPColor _kPulseColor = NDPColor.FromCCT(4000); // 4000 is the minimum value supported
    private const Channel _kChannel = Channel.Default;

    public StarPulseEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    private static Effect CreateEffect(LightId light, TimeSpan position, int totalLightCount)
    {
        int fadeDurationSeconds = totalLightCount switch
        {
            <= 6 => 1,
            <= 10 => 3,
            _ => 5
        };

        return new(light, position, duration: TimeSpan.Zero, _kPulseColor)
        {
            FadeOut = TimeSpan.FromSeconds(fadeDurationSeconds)
        };
    }

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(Channel.Default);
        if (channel is null)
            return;

        ClearChannelsForPulses(ctx, api);

        foreach (NDPInterval segment in ctx.Section.Timings.Segments)
        {
            TimeSpan pos = segment.Start;

            NDPLight[] availableLights = channel.GetAvailableLights(pos).ToArray();
            LightId light;
            if (availableLights.Length > 0)
                light = ctx.Random.Choice(availableLights).Id;
            else
                light = channel.GetBusyEffects(pos).MinBy(e => e.End).LightId;

            channel.Add(CreateEffect(light, pos, totalLightCount: channel.Lights.Count));
        }
    }

    private static void ClearChannelsForPulses(EffectContext ctx, EffectAPI api)
    {
        NDPInterval clearInterval = ctx.Section.Interval;

        // Use pulse color so that the color is consistent when interpolating brightness
        NDPColor resetColor = _kPulseColor.CopyWith(brightness: 0d);
        foreach (EffectChannel channel in api.Channels)
        {
            // Only clear channels that have priority lower than us
            // since if we clear _kChannel, GetAvailableLights() won't work
            // and if we clear channels after _kChannel, we override the effect with black.
            if (channel.Channel >= _kChannel)
                break;

            channel.Clear(clearInterval.Start, clearInterval.End);
            foreach (NDPLight light in channel.Lights)
                channel.Add(new Effect(light.Id, clearInterval.Start, clearInterval.Duration, resetColor));
        }
    }
}
