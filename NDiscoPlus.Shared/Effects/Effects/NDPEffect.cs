using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.Effects.Strobes;
using NDiscoPlus.Shared.Models;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared.Effects.Effects;

internal abstract class NDPEffect
{
    public static readonly ImmutableArray<NDPEffect> All = [
        BrightLightEffect.Default(EffectIntensity.High),
        BrightLightEffect.Slow(EffectIntensity.VeryLow),
        BrightLightEffect.White(EffectIntensity.VeryHigh),

        new ColorSwitchEffect(EffectIntensity.Medium),

        new GroupedStrobeLightEffect(GroupedStrobeLightEffect.GroupingType.Horizontal, EffectIntensity.Maximum),
        new GroupedStrobeLightEffect(GroupedStrobeLightEffect.GroupingType.Vertical, EffectIntensity.Maximum),
        new GroupedStrobeLightEffect(GroupedStrobeLightEffect.GroupingType.RandomPattern, EffectIntensity.Maximum),
        new RandomStrobeLightEffect(EffectIntensity.Maximum),

        new StarPulseEffect(EffectIntensity.Low)
    ];

    public static readonly IDictionary<EffectIntensity, IList<NDPEffect>> ByIntensity = Enum.GetValues<EffectIntensity>()
        .Select(i => new KeyValuePair<EffectIntensity, IList<NDPEffect>>(i, All.Where(eff => eff.Intensity == i).ToImmutableList()))
        .ToImmutableDictionary();

    public abstract void Generate(EffectContext ctx, EffectAPI api);


    /// <summary>An effect specialised in categorisation by intensity.</summary>
    /// <param name="intensity">Describes the intensity of this effect. 1 (lowest) - 5 (highest)</param>
    protected NDPEffect(EffectIntensity intensity)
    {
        Intensity = intensity;
    }

    public EffectIntensity Intensity { get; }
}