using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;

public class EffectConfig
{
    public static readonly EffectConfig Default = new();

    //                                   \/ not implemented
    public enum StrobeStyles { Instant, Realistic }

    /// <summary>
    /// The base brightness all lights are set as default.
    /// </summary>
    public double BaseBrightness { get; init; } = 0.1d;

    /// <summary>
    /// <para>The base brightness that should be used when effect doesn't manipulate light brightness.</para>
    /// <para>This is to ensure that when no lights are at max brightness, the scene isn't completely black.</para>
    /// </summary>
    public double EffectBaseBrightness { get; init; } = 0.35d;

    /// <summary>
    /// Reduced max brightness for eye comfort. As max brightness of 1d might be too harsh sometimes.
    /// </summary>
    public double ReducedMaxBrightness { get; init; } = 0.75d;

    // Removed as I most likely will forget to apply this instead of 1d as brightness
    // thus making this setting unusable.
    // I could remap the brightness in the light interpreter in the future if I need to reduce the max brightness
    // /// <summary>
    // /// Maximum brightness the lights are allowed to go.
    // /// </summary>
    // public double MaxBrightness { get; init; } = 1d;

    // Strobe light color is around 5600 kelvin:
    // - https://en.wikipedia.org/wiki/Strobe_light
    //    - "color temperature of approximately 5,600 kelvins"
    // - https://fixthephoto.com/best-strobe-lights-for-photography.html#:~:text=The%20best%20strobe%20lights%20for%20photography%20have%20a,modern%20DSLRs%20allow%20adjusting%20the%20expected%20color%20temperature.
    //    - ^ Check the linked products, they're pretty much all 5600
    public NDPColor StrobeColor { get; init; } = NDPColor.FromCCT.BlackBody(5600);
    public StrobeStyles StrobeStyle { get; init; } = StrobeStyles.Instant;
}