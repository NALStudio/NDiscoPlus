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

    public double BaseBrightness { get; init; } = 0.1d;
    public double MaxBrightness { get; init; } = 1d;

    // Strobe light color is around 5600 kelvin:
    // - https://en.wikipedia.org/wiki/Strobe_light
    // - https://fixthephoto.com/best-strobe-lights-for-photography.html#:~:text=The%20best%20strobe%20lights%20for%20photography%20have%20a,modern%20DSLRs%20allow%20adjusting%20the%20expected%20color%20temperature.
    //    - ^ Check the linked products, they're pretty much all 5600
    public NDPColor StrobeColor { get; init; } = NDPColor.FromCCT.BlackBody(5600);
    public StrobeStyles StrobeStyle { get; init; } = StrobeStyles.Instant;
}