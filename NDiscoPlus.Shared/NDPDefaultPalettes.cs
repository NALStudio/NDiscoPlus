using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using SkiaSharp;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared;

public static class NDPDefaultPalettes
{
    /// <summary>
    /// Contains four colors in the sRGB range.
    /// </summary>
    public static readonly NDPColorPalette DefaultSRGB = new(
        new SKColor(255, 0, 0),
        new SKColor(0, 255, 255),
        new SKColor(255, 105, 180),
        new SKColor(102, 51, 153)
    );

    /// <summary>
    /// Contains six colors in Philips Hue HDR range.
    /// </summary>
    public static readonly NDPColorPalette DefaultHDR = new(
        ColorGamut.hueGamutC.Red.ToColor(),
        NDPColor.Lerp(ColorGamut.hueGamutC.Red.ToColor(), ColorGamut.hueGamutC.Green.ToColor(), 0.5),
        ColorGamut.hueGamutC.Green.ToColor(),
        NDPColor.Lerp(ColorGamut.hueGamutC.Green.ToColor(), ColorGamut.hueGamutC.Blue.ToColor(), 0.5),
        ColorGamut.hueGamutC.Blue.ToColor(),
        NDPColor.Lerp(ColorGamut.hueGamutC.Blue.ToColor(), ColorGamut.hueGamutC.Red.ToColor(), 0.5)
    );

    public static readonly ImmutableArray<NDPColorPalette> SRGB = [
        DefaultSRGB,
        new NDPColorPalette(new SKColor(15, 192, 252), new SKColor(123, 29, 175), new SKColor(255, 47, 185), new SKColor(212, 255, 71)),
        new NDPColorPalette(new SKColor(255, 0, 0), new SKColor(0, 255, 0), new SKColor(0, 0, 255), new SKColor(255, 255, 0)),
        new NDPColorPalette(new SKColor(164, 20, 217), new SKColor(255, 128, 43), new SKColor(249, 225, 5), new SKColor(52, 199, 165), new SKColor(93, 80, 206)),
    ];

    public static readonly ImmutableArray<NDPColorPalette> HDR = [
        DefaultHDR
    ];

    public static NDPColorPalette GetRandomPalette(Random random, bool allowHDR)
    {
        int totalCount = SRGB.Length;
        if (allowHDR)
            totalCount += HDR.Length;

        int index = random.Next(totalCount);
        if (index < SRGB.Length)
            return SRGB[index];
        else
            return HDR[index];
    }
}