using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Models;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared.Effects.Strobes;

/// <summary>
/// NDPStrobes implement both strobes and flashes of NDiscoPlus.
/// </summary>
internal abstract class NDPStrobe
{
    public static readonly ImmutableArray<NDPStrobe> All = [
        new SegmentBurstStrobes()
    ];
    public static readonly ImmutableArray<NDPStrobe> BeforeEffects = All.Where(x => x.StrobeGeneration == StrobeGeneration.BeforeEffects).ToImmutableArray();
    public static readonly ImmutableArray<NDPStrobe> AfterEffects = All.Where(x => x.StrobeGeneration == StrobeGeneration.AfterEffects).ToImmutableArray();

    /// <summary>
    /// Control when strobes are generated in relation to effects.
    /// </summary>
    /// <remarks>
    /// <para>When using <see cref="StrobeGeneration.AfterEffects"/>, you must make sure to avoid drawing over any strobes and flashes already present in your channel of interest.</para>
    /// </remarks>
    public abstract StrobeGeneration StrobeGeneration { get; }
    public abstract void Generate(StrobeContext ctx, EffectAPI api);
}
