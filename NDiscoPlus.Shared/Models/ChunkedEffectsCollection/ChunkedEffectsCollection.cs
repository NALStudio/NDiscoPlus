using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public sealed partial class ChunkedEffectsCollection
{
    public const int CHUNK_SIZE_SECONDS = 1;

    public ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>> BackgroundTransitions { get; }

    private readonly ImmutableArray<Chunk> chunks;
    private readonly ImmutableArray<Effect> effects;
    private readonly ImmutableArray<NDPInterval> backgroundDisabled;

    public int ChunkCount => chunks.Length;
    public static TimeSpan ChunkSize => TimeSpan.FromSeconds(CHUNK_SIZE_SECONDS);

    private ChunkedEffectsCollection(ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>> backgroundTransitions, ImmutableArray<Chunk> chunks, ImmutableArray<Effect> effects, ImmutableArray<NDPInterval> backgroundDisabled)
    {
        BackgroundTransitions = backgroundTransitions;

        this.chunks = chunks;
        this.effects = effects;
        this.backgroundDisabled = backgroundDisabled;
    }

    private static int ToChunkIndex(TimeSpan time)
        => ToChunkIndex(time.TotalSeconds);

    private static int ToChunkIndex(double timeSeconds)
    {
        int index = (int)timeSeconds / CHUNK_SIZE_SECONDS;
        if (timeSeconds < 0) // if time is negative, decrement index so that value always rounds downwards (i.e. 0.001 => 0 and -0.001 => -1)
            index--;
        return index;
    }

    private bool TryGetChunk(TimeSpan time, [MaybeNullWhen(false)] out Chunk chunk)
    {
        int index = ToChunkIndex(time);
        if (index < 0 || index >= chunks.Length)
        {
            chunk = default;
            return false;
        }

        chunk = chunks[index];
        return true;
    }

    public IEnumerable<Effect> GetEffects(TimeSpan time)
    {
        if (TryGetChunk(time, out Chunk chunk))
        {
            foreach (int effectIndex in chunk.EffectIndexes)
                yield return effects[effectIndex];
        }
    }

    public bool GetBackgroundDisabled(TimeSpan time)
    {
        if (TryGetChunk(time, out Chunk chunk))
        {
            foreach (int disabledIndex in chunk.BackgroundDisabledIndexes)
            {
                NDPInterval disabled = backgroundDisabled[disabledIndex];
                if (disabled.Contains(time))
                    return true;
            }
        }

        return false;
    }

    private static void ConstructEffects(ChunkBuilder builder, ImmutableArray<Effect> effects)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            Effect e = effects[i];

            int startChunk = ToChunkIndex(e.Start);
            if (startChunk < 0)
                startChunk = 0;

            int endChunk = ToChunkIndex(e.End); // inclusive

            builder.ExtendToIndex(endChunk);
            for (int j = startChunk; j <= endChunk; j++)
                builder.At(j).AddEffect(i); // add effect index to chunk
        }
    }

    private static void ConstructBackgroundDisabled(ChunkBuilder builder, ImmutableArray<NDPInterval> backgroundDisabled)
    {
        for (int i = 0; i < backgroundDisabled.Length; i++)
        {
            NDPInterval disabled = backgroundDisabled[i];

            int startChunk = ToChunkIndex(disabled.Start);
            int endChunk = ToChunkIndex(disabled.End);

            builder.ExtendToIndex(endChunk);
            for (int j = startChunk; j <= endChunk; j++)
                builder.At(j).AddBackgroundDisabled(i);
        }
    }

    private static ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>> ConstructBackgroundTransitions(IEnumerable<BackgroundTransition>? transitionsUnordered)
    {
        if (transitionsUnordered is null)
            return ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>>.Empty;

        Dictionary<LightId, List<BackgroundTransition>> transitions = new();

        foreach (BackgroundTransition trans in transitionsUnordered)
        {
            if (!transitions.TryGetValue(trans.LightId, out List<BackgroundTransition>? ordered))
            {
                ordered = new();
                transitions.Add(trans.LightId, ordered);
            }

            Bisect.InsortRight(ordered, trans, static bt => bt.Start);
        }

        return transitions.ToImmutableDictionary(
            keySelector: static x => x.Key,
            elementSelector: static x => x.Value.ToImmutableArray()
        );
    }

    internal static ChunkedEffectsCollection Construct(EffectAPI effects)
    {
        ImmutableArray<Effect> allEffects = effects.Channels.SelectMany(c => c.Effects).ToImmutableArray();
        ImmutableArray<NDPInterval> backgroundDisabled = effects.Background?.DisabledIntervals.ToImmutableArray() ?? ImmutableArray<NDPInterval>.Empty;

        ChunkBuilder chunkBuilder = new();
        ConstructEffects(chunkBuilder, allEffects);
        ConstructBackgroundDisabled(chunkBuilder, backgroundDisabled);

        ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>> transitions = ConstructBackgroundTransitions(effects.Background?.TransitionsUnordered);

        return new ChunkedEffectsCollection(
            backgroundTransitions: transitions,
            chunks: chunkBuilder.Build(),
            effects: allEffects,
            backgroundDisabled: backgroundDisabled
        );
    }
}