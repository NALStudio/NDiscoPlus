using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.Effects.Strobes;
using NDiscoPlus.Shared.Models;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared.Effects.Effects;

/* ##### WHEN CREATING ENTERTAINMENT AREAS, PLEASE NOTE THE FOLLOWING #####
 * 
 * General:
 *   - Light X and Z positions are used when generating effects, Y is not.
 *       - This means that the best results can be achieved by creating an array of lights in front of the user.
 *       
 * ColorSwitchEffect
 *   - Lights are split into groups by distance. A new group is created each time the distance between lights is more than 0,2.
 *       - Best results can be achieved when groups are approximately equal in size.
 *       
 * ColorStripeEffect
 *   - Lights are split into groups by distance. Stripe starts from the leftmost light and follows the shortest path to the next light.
 *       - Best results can be achieved when this path is continuous and doesn't have large gaps between lights.
*/

internal abstract class NDPEffect
{
    public static readonly ImmutableArray<NDPEffect> All = [
        // Bright light effect
        BrightLightEffect.Default(EffectIntensity.High),
        BrightLightEffect.Slow(EffectIntensity.VeryLow),
        BrightLightEffect.White(EffectIntensity.VeryHigh),

        // Strobe effects
        new GroupedStrobeLightEffect(GroupedStrobeLightEffect.GroupingType.Horizontal, EffectIntensity.Maximum),
        new GroupedStrobeLightEffect(GroupedStrobeLightEffect.GroupingType.Vertical, EffectIntensity.Maximum),
        new GroupedStrobeLightEffect(GroupedStrobeLightEffect.GroupingType.RandomPattern, EffectIntensity.Maximum),
        // new RandomStrobeLightEffect(EffectIntensity.Maximum), // Deprecated
        new StrobeStripeEffect(EffectIntensity.Maximum),

        // Miscellaneous
        new ColorSwitchEffect(EffectIntensity.Medium),
        new FlashEffect(EffectIntensity.Medium),
        new ColorStripeEffect(EffectIntensity.Medium),

        new BrightLightSlowInstantEffect(EffectIntensity.Low),
        new StarPulseEffect(EffectIntensity.Low),
    ];

    public static readonly IDictionary<EffectIntensity, IList<NDPEffect>> ByIntensity = Enum.GetValues<EffectIntensity>()
        .Select(i => new KeyValuePair<EffectIntensity, IList<NDPEffect>>(i, All.Where(eff => eff.Intensity == i).ToImmutableList()))
        .ToImmutableDictionary();

    public abstract void Generate(EffectContext ctx, EffectAPI api);


    /// <summary>An effect specialised in categorization by intensity.</summary>
    /// <param name="intensity">Describes the intensity of this effect. 1 (lowest) - 5 (highest)</param>
    protected NDPEffect(EffectIntensity intensity)
    {
        Intensity = intensity;
    }

    public EffectIntensity Intensity { get; init; }
}