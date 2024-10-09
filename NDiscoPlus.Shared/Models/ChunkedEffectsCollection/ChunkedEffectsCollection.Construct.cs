using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public sealed partial class ChunkedEffectsCollection
{
    private class ChunkBuilder
    {
        private readonly List<Chunk> chunks;

        public ChunkBuilder()
        {
            chunks = new();
        }

        public void ExtendToIndex(int index)
        {
            while (chunks.Count < (index + 1)) // +1 since we compare index to count
                chunks.Add(new Chunk());
        }

        public Chunk At(int index) => chunks[index];

        public ImmutableArray<Chunk> Build() => chunks.ToImmutableArray();
    }

    private static void ConstructBackground(ref readonly ChunkBuilder builder, ImmutableArray<BackgroundTransition> transitions)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            BackgroundTransition t = transitions[i];

            int startChunk = ToChunkIndex(t.Start);
            int endChunk = ToChunkIndex(t.End);

            builder.ExtendToIndex(endChunk);
            for (int j = startChunk; j <= endChunk; j++)
                builder.At(j).AddBackgroundTransition(i);
        }
    }

    private static void ConstructDisabled(ref readonly ChunkBuilder builder, ImmutableArray<NDPInterval> disabled)
    {
        foreach (NDPInterval d in disabled)
        {
            int startChunk = ToChunkIndex(d.Start);
            int endChunk = ToChunkIndex(d.End);

            builder.ExtendToIndex(endChunk);
            for (int j = startChunk; j <= endChunk; j++)
                builder.At(j).SetDisabled(d);
        }
    }

    private static void ContructEffects(ref readonly ChunkBuilder builder, ImmutableArray<Effect> effects)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            Effect e = effects[i];

            double startTotalSeconds = e.Start.TotalSeconds;
            // Check if over or equal to 0 since if effect position is 0 and it has a fade-in, the start goes below 0.
            int startChunk = startTotalSeconds >= 0d ? ToChunkIndex(startTotalSeconds) : 0;
            int endChunk = ToChunkIndex(e.End); // inclusive

            builder.ExtendToIndex(endChunk);
            for (int j = startChunk; j <= endChunk; j++)
                builder.At(j).AddEffect(i); // add effect index to chunk
        }
    }

    internal static ChunkedEffectsCollection Construct(EffectAPI effects)
    {
        ImmutableArray<Effect> allEffects = effects.Channels.SelectMany(c => c.Effects).ToImmutableArray();

        ImmutableArray<NDPInterval> disabled = effects.Background?.DisabledIntervals.ToImmutableArray() ?? ImmutableArray<NDPInterval>.Empty;
        ImmutableArray<BackgroundTransition> backgroundTransitions = effects.Background?.Transitions.ToImmutableArray() ?? ImmutableArray<BackgroundTransition>.Empty;

        ChunkBuilder builder = new();
        if (effects.Background is not null)
            ConstructBackground(ref builder, backgroundTransitions);
        ConstructDisabled(ref builder, disabled);
        ContructEffects(ref builder, allEffects);

        return new ChunkedEffectsCollection(
            effects: allEffects,
            backgroundTransitions: backgroundTransitions,
            chunks: builder.Build()
        );
    }
}
