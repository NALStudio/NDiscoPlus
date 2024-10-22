using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Strobes;
internal class SegmentHighlightFlashes : NDPStrobe
{
    public override StrobeGeneration StrobeGeneration => StrobeGeneration.AfterEffects;

    public override void Generate(StrobeContext ctx, EffectAPI api)
    {
        foreach (ImmutableArray<NDPInterval> highlight in ctx.Analysis.Segments.Highlights)
            GenerateForHighlight(ctx, api, highlight);
    }

    private static bool IsChannelBusyDuringHighlight(EffectChannel? channel, ImmutableArray<NDPInterval> highlight)
    {
        if (channel is null)
            return false;

        TimeSpan start = highlight[0].Start;
        TimeSpan end = highlight[^1].End;

        Debug.Assert(start == highlight.Min(inter => inter.Start));
        Debug.Assert(end == highlight.Max(inter => inter.End));

        NDPInterval burstTotal = NDPInterval.FromStartAndEnd(start, end);
        return channel.GetBusyEffects(burstTotal).Any();
    }

    private static void GenerateForHighlight(StrobeContext ctx, EffectAPI api, ImmutableArray<NDPInterval> highlight)
    {
        if (IsChannelBusyDuringHighlight(api.GetChannel(Channel.Strobe), highlight))
            return;

        EffectChannel? channel = api.GetChannel(Channel.Flash);
        if (channel is null)
            return;
        if (IsChannelBusyDuringHighlight(channel, highlight))
            return;

        int groupCount = SegmentBurstStrobes.ReduceGroupCount(highlight.Length);
        List<NDPLight[]> lightGroups = channel.Lights.GroupX(groupCount);

        int colorIndex = ctx.Random.Next(ctx.Palette.Count);

        for (int i = 0; i < highlight.Length; i++)
        {
            NDPInterval h = highlight[i];

            NDPColor color = ctx.Palette[(colorIndex + i) % ctx.Palette.Count]
                                .CopyWith(brightness: 1d);

            foreach (NDPLight light in lightGroups[i % groupCount])
            {
                channel.Add(
                    new Effect(
                        light.Id,
                        h.Start,
                        h.Duration,
                        color
                    )
                );
            }
        }
    }
}
