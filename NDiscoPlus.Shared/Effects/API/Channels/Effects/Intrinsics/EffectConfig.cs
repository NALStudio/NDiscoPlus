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

    // Strobe lights mimic the color of daylight at around 6500 kelvin: https://en.wikipedia.org/wiki/Strobe_light
    // As such we can use D65 as our strobe color (2° observer): https://en.wikipedia.org/wiki/Standard_illuminant#D65_values
    public NDPColor StrobeColor { get; init; } = new(0.31272d, 0.32903d, 1d);
    public StrobeStyles StrobeStyle { get; init; } = StrobeStyles.Instant;
}