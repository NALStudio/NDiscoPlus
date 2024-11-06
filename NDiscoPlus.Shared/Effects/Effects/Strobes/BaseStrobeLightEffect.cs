﻿using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Effects.Strobes;
internal abstract class BaseStrobeLightEffect : NDPEffect
{
    protected readonly struct LightGroup : IEnumerable<LightId>
    {
        private readonly ImmutableArray<LightId> lights;

        public LightGroup()
        {
            lights = ImmutableArray<LightId>.Empty;
        }

        public LightGroup(ImmutableArray<LightId> lights)
        {
            this.lights = lights;
        }

        public static LightGroup UnsafeCast(LightId[] lights) => new(ImmutableCollectionsMarshal.AsImmutableArray(lights));

        public static LightGroup FromIds(IEnumerable<LightId> lightIds) => new(lightIds.ToImmutableArray());
        public static LightGroup FromLights(IEnumerable<NDPLight> lights) => FromIds(lights.Select(l => l.Id));

        public IEnumerator<LightId> GetEnumerator() => ((IEnumerable<LightId>)lights).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    protected BaseStrobeLightEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    /// <summary>
    /// <para>Divide by 2 and round upwards. Same as ceil(x / 2).</para>
    /// <para>We will use special notation <c>^/</c> in the docs to refer to this division.</para>
    /// <para>NOTE: <c>1 ^/ 2 = 1</c> and <c>0 ^/ 2 = 0</c>. This means you cannot divide indefinitely.</para>
    /// </summary>
    private static int DivideBy2RoundUpwards(int x)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(x, 0, nameof(x));

        // 0 / 2 => 0 rem 0 => 0
        // 1 / 2 => 0 rem 1 => 1
        // 2 / 2 => 1 rem 0 => 1
        // 3 / 2 => 1 rem 1 => 2
        // 4 / 2 => 2 rem 0 => 2
        // 5 / 2 => 2 rem 1 => 3
        // 6 / 2 => 3 rem 0 => 3
        (int quotient, int remainder) = Math.DivRem(x, 2);
        if (remainder == 0)
            return quotient;
        else
            return quotient + 1; // 3 / 2 => 1 + 1 => 2
    }

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(Channel.Strobe);
        if (channel is null)
            return;

        NDPLightCollection lights = channel.Lights;

        (int groupCount, ImmutableArray<NDPInterval> syncIntervals) = GenerateSyncIntervals(ctx);

        ClearChannelsForStrobes(api, syncIntervals);

        int frameCount = syncIntervals.Length;
        ImmutableArray<LightGroup> groups = Group(ctx, lights, frameCount, groupCount).ToImmutableArray();

        GenerateStrobes(api.Config, channel, syncIntervals, groups);
    }

    /// <summary>
    /// Group the lights into n groups.
    /// </summary>
    /// <param name="frameCount">How many frames have been generated by the effect.</param>
    /// <param name="groupCount">How many groups are recommended for the effect.</param>
    /// <returns>
    /// <para>The amount of groups the implementer deems necessary.</para>
    /// <para>Recommended amount are either <paramref name="frameCount"/> or <paramref name="groupCount"/>.</para>
    /// <para>These values are iterated in a loop until all frames have a designated group.</para>
    /// </returns>
    protected abstract IEnumerable<LightGroup> Group(EffectContext ctx, NDPLightCollection lights, int frameCount, int groupCount);

    public static (int GroupCount, ImmutableArray<NDPInterval> SyncIntervals) GenerateSyncIntervals(EffectContext ctx)
    {
        int effectsPerSync = SyncIntervalEffectsPerSync(ctx.Section.Tempo.TimeSignature, ctx.Section.Timings.Tatums);
        IList<NDPInterval> syncIntervals = ctx.Section.Timings.Tatums;

        if (effectsPerSync == 0)
        {
            effectsPerSync = SyncIntervalEffectsPerSync(ctx.Section.Tempo.TimeSignature, ctx.Section.Timings.Beats);
            syncIntervals = ctx.Section.Timings.Beats;
        }
        if (effectsPerSync == 0)
        {
            effectsPerSync = SyncIntervalEffectsPerSync(ctx.Section.Tempo.TimeSignature, ctx.Section.Timings.Bars);
            syncIntervals = ctx.Section.Timings.Bars;
        }
        if (effectsPerSync == 0)
            throw new InvalidOperationException($"Sync not possible. Time signature: {ctx.Section.Tempo.TimeSignature}/4");

        int groupCount = SyncIntervalGroupCount(ctx.Section.Tempo.TimeSignature, effectsPerSync);
        if (groupCount < 3)
            groupCount += 2; // Group count 1 => 3, group count 2 => 4. This is done so that the lights don't just spasm and actually display proper strobes
        Debug.Assert(groupCount >= 3); // Just in case we get an unexpected group count

        int totalEffectsCount = syncIntervals.Count * effectsPerSync;
        NDPInterval[] effects = new NDPInterval[totalEffectsCount];
        for (int i = 0; i < effects.Length; i++)
        {
            (int intervalIndex, int effectIndex) = Math.DivRem(i, effectsPerSync);
            NDPInterval syncInterval = syncIntervals[intervalIndex];

            TimeSpan partDuration = syncIntervals[intervalIndex].Duration / effectsPerSync;
            TimeSpan partStart = syncInterval.Start + (effectIndex * partDuration);

            effects[i] = new(partStart, partDuration);
        }

        return (groupCount, ImmutableCollectionsMarshal.AsImmutableArray(effects));
    }

    private static int SyncIntervalEffectsPerSync(int timeSignature, IList<NDPInterval> syncIntervals)
    {
        int effectsPerSync = timeSignature;
        while (syncIntervals.Any(interval => (interval.Duration / effectsPerSync) <= EffectConstants.MinEffectDuration))
        {
            // DivideBy2RoundUpwards(1) => 1 => infinite loop
            // as we cannot converge into any value after that, we return 0
            if (effectsPerSync > 1)
                effectsPerSync = DivideBy2RoundUpwards(effectsPerSync);
            else
                return 0;
        }

        return effectsPerSync;
    }

    private static int SyncIntervalGroupCount(int timeSignature, int effectsPerSync)
    {
        // Choose a group count where it syncs up with effects nicely.
        // This is possible using the song's timeSignature when effectsPerSync is 1 OR effectsPerSync and timeSignature are both divisible by 2
        // otherwise we must have the group count equal to effects per sync
        if (effectsPerSync == 1 || (effectsPerSync % 2 == 0 && timeSignature % 2 == 0))
            return timeSignature;
        else
            return effectsPerSync;
    }

    private static void GenerateStrobes(EffectConfig config, EffectChannel channel, ImmutableArray<NDPInterval> syncIntervals, ImmutableArray<LightGroup> groups)
    {
        for (int i = 0; i < syncIntervals.Length; i++)
        {
            NDPInterval interval = syncIntervals[i];
            foreach (LightId light in groups[i % groups.Length])
                channel.Add(Effect.CreateStrobe(config, light, interval));
        }
    }

    public static void ClearChannelsForStrobes(EffectAPI api, ImmutableArray<NDPInterval> timings)
    {
        TimeSpan clearStart = timings.Min(static t => t.Start);
        TimeSpan clearEnd = timings.Max(static t => t.End);
        TimeSpan clearLength = clearEnd - clearStart;

        // Use strobe color so that the color is consistent when interpolating brightness
        NDPColor strobeResetColor = api.Config.StrobeColor.CopyWith(brightness: 0d);
        foreach (EffectChannel channel in api.Channels)
        {
            // We want to clear absolutely everything for strobes
            // Although we don't need to disable the background animation anymore as this method definitely handles that already :D
            channel.Clear(clearStart, clearEnd);
            foreach (NDPLight light in channel.Lights)
                channel.Add(new Effect(light.Id, clearStart, clearLength, strobeResetColor));
        }
    }
}