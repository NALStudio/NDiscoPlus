using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public sealed partial class ChunkedEffectsCollection
{
    public const int CHUNK_SIZE_SECONDS = 1;

    private readonly ImmutableArray<Effect> effects;
    private readonly ImmutableArray<BackgroundTransition> backgroundTransitions;
    private readonly ImmutableArray<Chunk> chunks;

    private ChunkedEffectsCollection(ImmutableArray<Effect> effects, ImmutableArray<BackgroundTransition> backgroundTransitions, ImmutableArray<Chunk> chunks)
    {
        this.effects = effects;
        this.backgroundTransitions = backgroundTransitions;
        this.chunks = chunks;
    }

    // public int ChunkCount => chunks.Length;
    // public static TimeSpan ChunkSize => TimeSpan.FromSeconds(CHUNK_SIZE_SECONDS);

    private static int ToChunkIndex(TimeSpan time)
        => ToChunkIndex(time.TotalSeconds);

    private static int ToChunkIndex(double timeSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeSeconds, 0d, nameof(timeSeconds));
        return (int)timeSeconds / CHUNK_SIZE_SECONDS;
    }

    private bool InternalTryGetChunk(TimeSpan time, [MaybeNullWhen(false)] out Chunk chunk)
    {
        int index = ToChunkIndex(time);
        if (index >= chunks.Length)
        {
            chunk = default;
            return false;
        }

        chunk = chunks[index];
        return true;
    }

    public ChunkView GetChunk(TimeSpan time)
    {
        if (InternalTryGetChunk(time, out Chunk? chunk))
            return new ChunkView(chunk, this);
        else
            return new ChunkView(null, this);
    }
}