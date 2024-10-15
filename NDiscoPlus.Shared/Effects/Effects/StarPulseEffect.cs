using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Effects;

internal class StarPulseEffect : NDPEffect
{
    private static readonly NDPColor _kPulseColor = NDPColor.FromCCT.BlackBody(2500);
    private const Channel _kChannel = Channel.Default;

    public StarPulseEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    private static Effect CreateEffect(LightId light, TimeSpan position, int totalLightCount)
    {
        // TODO: Scale based on tempo and light count?
        int fadeDurationSeconds = totalLightCount switch
        {
            <= 6 => 1,
            <= 10 => 2,
            _ => 4
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

        // TODO: Filter out silent segments so that we don't spam lights during quiet parts
        // (I'll do this once I get my debugging tools back into a working state)
        foreach (NDPInterval segment in ctx.Section.Timings.Segments)
        {
            TimeSpan pos = segment.Start;

            NDPLight[] availableLights = channel.GetAvailableLights(pos).ToArray();
            LightId light;
            // must be over 1 (aka. >= 2) so that if there is only one light available,
            // we don't enter into an endless loop of the same pattern
            if (availableLights.Length > 1)
            {
                light = ctx.Random.Choice(availableLights).Id;
            }
            else
            {
                // Random select from two lowest value lights instead of taking the minimum value
                // so that the pattern is random (instead of a repeating pattern) in case the segments go by two fast
                List<LightId> almostFinishedLights = channel.GetBusyEffects(pos)
                                                           .OrderBy(e => e.End)
                                                           .Take(2)
                                                           .Select(static l => l.LightId)
                                                           .ToList();

                // Add back all lights that were ignored because we don't want to keep following the same pattern
                foreach (NDPLight wasAvailableButIgnored in availableLights)
                    almostFinishedLights.Add(wasAvailableButIgnored.Id);

                light = ctx.Random.Choice(almostFinishedLights);
            }

            channel.Add(CreateEffect(light, pos, totalLightCount: channel.Lights.Count));
        }
    }

    private static void ClearChannelsForPulses(EffectContext ctx, EffectAPI api)
    {
        // TODO: Consider the new background disable API
        NDPInterval clearInterval = ctx.Section.Interval;

        // Use pulse color so that the color is consistent when interpolating brightness
        NDPColor resetColor = _kPulseColor.CopyWith(brightness: 0d);
        foreach (EffectChannel channel in api.Channels)
        {
            // Only clear channels that have priority lower than us
            // since if we clear _kChannel, GetAvailableLights() won't work
            // and if we clear channels after _kChannel, we override this effect with black.
            if (channel.Channel >= _kChannel)
                break;

            // Cannot be consolidated into a single function with strobe lights since we filter out specific channels
            channel.Clear(clearInterval.Start, clearInterval.End);
            foreach (NDPLight light in channel.Lights)
                channel.Add(new Effect(light.Id, clearInterval.Start, clearInterval.Duration, resetColor));
        }
    }
}
