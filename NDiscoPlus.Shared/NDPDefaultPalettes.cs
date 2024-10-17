using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using SkiaSharp;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared;

public static class NDPDefaultPalettes
{
    public static NDPColorPalette DefaultSRGB => SRGB[0];
    public static NDPColorPalette DefaultHDR => HDR[0];

    public static readonly ImmutableArray<NDPColorPalette> SRGB = [
        new NDPColorPalette(
            new SKColor(255, 0, 0), // red
            new SKColor(0, 255, 255), // cyan
            new SKColor(255, 105, 180), // pink
            new SKColor(102, 51, 153) // purple
        ),

        new NDPColorPalette(
            new SKColor(15, 192, 252), // cyan
            new SKColor(123, 29, 175), // purple
            new SKColor(255, 47, 185), // pink
            // new SKColor(212, 255, 71)  // !! piss yellow !!
            new SKColor(252, 237, 15) // switch piss yellow into a brighter yellow so that it looks a bit nicer
        ),

        // Basically just DefaultHDR, but uglier
        // new NDPColorPalette(
        //     new SKColor(255, 0, 0), // red
        //     new SKColor(0, 255, 0), // green
        //     new SKColor(0, 0, 255), // blue
        //     new SKColor(255, 255, 0) // yellow :(
        // ),

        new NDPColorPalette(
            new SKColor(164, 20, 217), // purple
            new SKColor(255, 128, 43), // orange
            // new SKColor(249, 225, 5),  // yellow
            new SKColor(52, 199, 165), // dark-ish cyan
            new SKColor(93, 80, 206)   // purple
        ),
    ];

    public static readonly ImmutableArray<NDPColorPalette> HDR = [
        new NDPColorPalette(
            ColorGamut.hueGamutC.Red.ToColor(),
            NDPColor.Lerp(ColorGamut.hueGamutC.Red.ToColor(), ColorGamut.hueGamutC.Green.ToColor(), 0.5),
            ColorGamut.hueGamutC.Green.ToColor(),
            NDPColor.Lerp(ColorGamut.hueGamutC.Green.ToColor(), ColorGamut.hueGamutC.Blue.ToColor(), 0.5),
            ColorGamut.hueGamutC.Blue.ToColor(),
            NDPColor.Lerp(ColorGamut.hueGamutC.Blue.ToColor(), ColorGamut.hueGamutC.Red.ToColor(), 0.5)
        ),
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
            return HDR[index - SRGB.Length];
    }
}