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
internal class FlashEffect : NDPEffect
{
    private const Channel _kChannel = Channel.Flash;

    public FlashEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    private class LightGroup
    {
        public ImmutableArray<NDPLight> Lights { get; init; }
        public NDPColor Color { get; set; }

        private LightGroup(ImmutableArray<NDPLight> lights, NDPColor color)
        {
            Lights = lights;
            Color = color;
        }

        public static LightGroup Create(EffectContext ctx, IEnumerable<NDPLight> lights)
        {
            return new(lights.ToImmutableArray(), ctx.Random.Choice(ctx.Palette));
        }
    }

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(_kChannel);
        if (channel is null)
            return;

        ClearChannelsForFlashes(ctx, api);

        (NDPLight[], NDPLight[]) grouped = GroupLights(channel);
        LightGroup[] groups = [LightGroup.Create(ctx, grouped.Item1), LightGroup.Create(ctx, grouped.Item2)];

        ImmutableArray<NDPInterval> beats = ctx.Section.Timings.Beats;

        for (int i = 0; i < beats.Length; i++)
        {
            NDPInterval beat = beats[i];
            LightGroup currentGroup = groups[i % groups.Length];

            NDPColor newColor;
            do
            {
                newColor = ctx.Random.Choice(ctx.Palette);
            } while (newColor == currentGroup.Color);
            currentGroup.Color = newColor;

            // Update all lights with background color
            foreach (LightGroup g in groups)
            {
                foreach (NDPLight l in g.Lights)
                {
                    channel.Add(
                        new Effect(
                            l.Id,
                            beat.Start,
                            beat.Duration
                        )
                        {
                            X = g.Color.X,
                            Y = g.Color.Y,
                            Brightness = api.Config.EffectBaseBrightness
                        }
                    );
                }
            }

            // Update current group with brightness 1
            // I'm not sure if this needs to be after color set or not, but I'll keep it here just in case
            foreach (NDPLight light in currentGroup.Lights)
            {
                channel.Add(
                    new Effect(
                        light.Id,
                        beat.Start,
                        EffectConstants.MinEffectDuration,
                        brightness: api.Config.ReducedMaxBrightness
                    )
                );
            }
        }
    }

    private static (NDPLight[], NDPLight[]) GroupLights(EffectChannel channel)
    {
        List<NDPLight[]> lightsSplit = channel.Lights.SplitX(tolerance: 0.2);
        if (lightsSplit.Count < 2) // If lights couldn't be split, force split into 3 groups
            lightsSplit = channel.Lights.GroupX(3);

        List<NDPLight> group1 = new(capacity: lightsSplit.Count); // I'd rather overallocate than reallocate
        List<NDPLight> group2 = new(capacity: lightsSplit.Count);
        if (lightsSplit.Count % 2 != 0) // Grouping is different based on if light count is even or odd
        {
            // When odd, intertwine lightSplit groups
            for (int i = 0; i < lightsSplit.Count; i++)
            {
                NDPLight[] lightSplitGroup = lightsSplit[i];
                if (i % 2 == 0)
                    group1.AddRange(lightSplitGroup);
                else
                    group2.AddRange(lightSplitGroup);
            }
        }
        else
        {
            // When even, intertwine but mirror at the centre
            // so basically we construct from the centre outwards
            int centerLeft = lightsSplit.Count / 2; // rounds downwards
            int centerRight = centerLeft + 1;

            int leftDistance = centerLeft + 1;
            int rightDistance = lightsSplit.Count - centerRight;
            Debug.Assert(leftDistance == rightDistance); // lightSplit.Count is even, so distance to both directions should be the same

            for (int i = 0; i < leftDistance; i++)
            {
                NDPLight[] leftGroup = lightsSplit[centerLeft - i];
                NDPLight[] rightGroup = lightsSplit[centerRight + i];
                if (i % 2 == 0)
                {
                    group1.AddRange(leftGroup);
                    group1.AddRange(rightGroup);
                }
                else
                {
                    group2.AddRange(leftGroup);
                    group2.AddRange(rightGroup);
                }
            }
        }

        return (group1.ToArray(), group2.ToArray());
    }

    private static void ClearChannelsForFlashes(EffectContext ctx, EffectAPI api)
    {
        // Not using the new background disable API
        // as I don't want the previous effect's effects to mess with ours

        NDPInterval clearInterval = ctx.Section.Interval;

        foreach (EffectChannel channel in api.Channels)
        {
            // Only clear channels that have priority lower than or equal to us
            // and if we clear channels after _kChannel, we override this effect with black.
            if (channel.Channel > _kChannel)
                break;

            // Cannot be consolidated into a single function with strobe lights since we filter out specific channels
            channel.Clear(clearInterval.Start, clearInterval.End);
            foreach (NDPLight light in channel.Lights)
            {
                // reset color doesn't matter as we don't interpolate between colors
                NDPColor resetColor = light.ColorGamut.GamutBlack();
                channel.Add(new Effect(light.Id, clearInterval.Start, clearInterval.Duration, resetColor));
            }
        }
    }
}
