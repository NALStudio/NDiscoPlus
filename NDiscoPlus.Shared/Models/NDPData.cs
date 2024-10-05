using Microsoft.AspNetCore.WebUtilities;
using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Spotify.Models;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NDiscoPlus.Shared.Models;

public class NDPData
{
    internal NDPData(
        SpotifyPlayerTrack track,
        NDPColorPalette referencePalette, NDPColorPalette effectPalette,
        EffectConfig effectConfig, ChunkedEffectsCollection effects,
        ImmutableArray<LightRecord> lights
    )
    {
        Track = track;
        ReferencePalette = referencePalette;
        EffectPalette = effectPalette;

        EffectConfig = effectConfig;
        Effects = effects;

        Lights = lights;
    }

    public SpotifyPlayerTrack Track { get; }

    public NDPColorPalette ReferencePalette { get; }
    public NDPColorPalette EffectPalette { get; }

    public EffectConfig EffectConfig { get; }
    public ChunkedEffectsCollection Effects { get; }

    public ImmutableArray<LightRecord> Lights { get; }
}


internal readonly struct EffectChunk
{
    private readonly List<int> effectIndexes;
    public IEnumerable<int> EffectIndexes => effectIndexes.AsReadOnly();

    public void AddIndex(int index)
        => effectIndexes.Add(index);

    public EffectChunk()
    {
        effectIndexes = new();
    }
}

public sealed class ChunkedEffectsCollection
{
    public const int CHUNK_SIZE_SECONDS = 1;

    public ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>> BackgroundTransitions { get; }

    private readonly ImmutableArray<Effect> effects;
    private readonly ImmutableArray<EffectChunk> chunks;

    public int ChunkCount => chunks.Length;
    public static TimeSpan ChunkSize => TimeSpan.FromSeconds(CHUNK_SIZE_SECONDS);

    private ChunkedEffectsCollection(ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>> backgroundTransitions, ImmutableArray<Effect> effects)
    {
        BackgroundTransitions = backgroundTransitions;
        this.effects = effects;
        chunks = ConstructChunks(effects);
    }

    private static int ToChunkIndex(TimeSpan time)
        => ToChunkIndex(time.TotalSeconds);

    private static int ToChunkIndex(double timeSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeSeconds, 0d, nameof(timeSeconds));
        return (int)timeSeconds / CHUNK_SIZE_SECONDS;
    }

    private bool TryGetChunk(TimeSpan time, [MaybeNullWhen(false)] out EffectChunk chunk)
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

    private IEnumerable<Effect> GetEffects(EffectChunk chunk)
    {
        foreach (int effectIndex in chunk.EffectIndexes)
            yield return effects[effectIndex];
    }

    public IEnumerable<Effect> GetEffectsOfChunk(int chunkIndex)
    {
        if (chunkIndex < 0 || chunkIndex >= chunks.Length)
            throw new ArgumentOutOfRangeException(nameof(chunkIndex));

        return GetEffects(chunks[chunkIndex]);
    }

    public IEnumerable<Effect> GetEffects(TimeSpan time)
    {
        if (TryGetChunk(time, out EffectChunk chunk))
            return GetEffects(chunk);
        else
            return Enumerable.Empty<Effect>();
    }

    private static ImmutableArray<EffectChunk> ConstructChunks(ImmutableArray<Effect> effects)
    {
        List<EffectChunk> chunks = new();
        for (int i = 0; i < effects.Length; i++)
        {
            Effect e = effects[i];

            double startTotalSeconds = e.Start.TotalSeconds;
            int startChunk = startTotalSeconds >= 0d ? ToChunkIndex(startTotalSeconds) : 0;
            int endChunk = ToChunkIndex(e.End); // inclusive

            while (chunks.Count < (endChunk + 1))
                chunks.Add(new EffectChunk());

            for (int j = startChunk; j <= endChunk; j++)
                chunks[j].AddIndex(i); // add effect index to chunk
        }

        return chunks.ToImmutableArray();
    }

    internal static ChunkedEffectsCollection Construct(EffectAPI effects)
    {
        ImmutableArray<Effect> allEffects = effects.Channels.SelectMany(c => c.Effects).ToImmutableArray();

        return new ChunkedEffectsCollection(
            backgroundTransitions: effects.Background?.Freeze() ?? ImmutableDictionary<LightId, ImmutableArray<BackgroundTransition>>.Empty,
            effects: allEffects
        );
    }
}