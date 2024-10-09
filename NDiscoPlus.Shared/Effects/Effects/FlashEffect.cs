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

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(_kChannel);
        if (channel is null)
            return;

        (NDPLight[] Group1, NDPLight[] Group2) = GroupLights(channel);

        ImmutableArray<NDPInterval> beats = ctx.Section.Timings.Beats;

        NDPColor? previouslyShown = null;
        for (int i = 0; i < beats.Length; i++)
        {
            NDPColor color;
            do
            {
                color = ctx.Random.Choice(ctx.Palette);
            } while (color == previouslyShown);

            NDPLight[] group = (i % 2 == 0) ? Group1 : Group2;
            aslkdjflkasdjlkfj lkajsd NOT FINISHED
        }

        // TODO: Use the new background disable API instead of ClearChannelsForStrobes
    }

    private (NDPLight[], NDPLight[]) GroupLights(EffectChannel channel)
    {
        List<NDPLight[]> lightsSplit = channel.Lights.SplitX(tolerance: 0.2);

        List<NDPLight> group1 = new(capacity: lightsSplit.Count); // I'd rather overallocate than reallocate
        List<NDPLight> group2 = new(capacity: lightsSplit.Count);
        if (lightsSplit.Count % 2 == 0) // Grouping is different based on if light count is even or odd
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

    private static void ClearChannelsForStrobes(EffectContext ctx, EffectAPI api)
    {
        // we sync using beats currently, but this might change in the future
        NDPInterval lastSyncObject = ctx.Section.Timings.Beats[^1];
        TimeSpan strobeEnd = lastSyncObject.End;
        // Debug.Assert(strobeEnd >= ctx.End); This assert seemed to cause some crashes

        TimeSpan clearStart = ctx.Section.Interval.Start;
        TimeSpan clearEnd = strobeEnd;
        TimeSpan clearLength = clearEnd - clearStart;

        // Use strobe color as the tint of the black color doesn't matter
        // (animation duration is 0 so we don't interpolate between colors)
        NDPColor strobeResetColor = api.Config.StrobeColor.CopyWith(brightness: 0d);
        foreach (EffectChannel channel in api.Channels)
        {
            // Do not clear any channels beyond our channel or we will draw black over this effect
            // This also allows strobes to pass through which would look nice :)
            if (channel.Channel <= _kChannel)
                channel.Clear(clearStart, clearEnd);
            // Technically I could pass flashes through if all of the lights in the flash channel are overridden from the earlier channels
            // but I'm not sure this is always the case... which is why I'm a bit hesitant on this.

            foreach (NDPLight light in channel.Lights)
                channel.Add(new Effect(light.Id, clearStart, clearLength, strobeResetColor));
        }
    }
}
