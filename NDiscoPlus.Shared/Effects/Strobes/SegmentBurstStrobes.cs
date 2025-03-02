﻿using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Music;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NDiscoPlus.Shared.Effects.Strobes;

internal class SegmentBurstStrobes : NDPStrobe
{
    public override StrobeGeneration StrobeGeneration => StrobeGeneration.AfterEffects;

    public override void Generate(StrobeContext ctx, EffectAPI api)
    {
        foreach (ImmutableArray<NDPInterval> burst in ctx.Analysis.Segments.Bursts)
        {
            if (IsEffectIntenseEnoughForBurst(ctx, burst))
                GenerateForBurst(api, burst);
        }
    }

    private static bool IsEffectIntenseEnoughForBurst(StrobeContext ctx, ImmutableArray<NDPInterval> burst)
    {
        NDPInterval burstInterval = NDPInterval.FromStartAndEnd(burst[0].Start, burst[^1].End);
        return ctx.IntensityAtLeast(EffectIntensity.Medium, burstInterval);
    }

    private static bool IsChannelBusyDuringBurst(EffectChannel channel, ImmutableArray<NDPInterval> burst)
    {
        TimeSpan burstStart = burst[0].Start;
        TimeSpan burstEnd = burst[^1].End;

        Debug.Assert(burstStart == burst.Min(inter => inter.Start));
        Debug.Assert(burstEnd == burst.Max(inter => inter.End));

        NDPInterval burstTotal = NDPInterval.FromStartAndEnd(burstStart, burstEnd);
        return channel.GetBusyEffects(burstTotal).Any();
    }

    private static void GenerateForBurst(EffectAPI api, ImmutableArray<NDPInterval> burst)
    {
        EffectChannel? channel = api.GetChannel(Channel.Strobe);
        if (channel is null)
            return;
        if (IsChannelBusyDuringBurst(channel, burst))
            return;

        int groupCount = ReduceGroupCount(burst.Length);
        List<NDPLight[]> lightGroups = channel.Lights.GroupX(groupCount);

        // Console.WriteLine(groupCount);
        // Console.WriteLine(groupCount);
        // Console.WriteLine(groupCount);
        // foreach (NDPLight[] group in lightGroups)
        // {
        //     Console.WriteLine("[");
        //     foreach (NDPLight light in group)
        //         Console.WriteLine($"    {light.Position.X}");
        //     Console.WriteLine("]");
        // }

        Debug.Assert(lightGroups.Count == groupCount);

        for (int i = 0; i < burst.Length; i++)
        {
            NDPInterval b = burst[i];
            foreach (NDPLight light in lightGroups[i % groupCount])
                channel.Add(Effect.CreateStrobe(api.Config, light.Id, b));
        }
    }

    public static int ReduceGroupCount(int groupCount)
    {
        // cases (when reducing with 5, 4 and 3):
        //   10 => 5
        //   9 => 3
        //   8 => 4
        //   7 => default
        //   6 => 3
        //   5 => 5
        //   4 => 4
        //   3 => 3
        //  <3 => default

        // reduce the groups to a more manageable count
        if (groupCount % 5 == 0)
            return 5;
        if (groupCount % 4 == 0)
            return 4;
        // if (groupCount % 3 == 0)
        //     return 3;
        // return 2;

        // Use minimum of 3 instead since with 2, the lights didn't have enough time to turn off before they needed to turn back on thus they looking like a spasming piece of mess
        return 3;
    }
}
