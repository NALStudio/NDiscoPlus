using NDiscoPlus.Shared.Helpers;

namespace NDiscoPlus.Shared.Models.Color;
public readonly partial struct NDPColor
{
    public NDPColor Clamp(ColorGamut gamut)
    {
        ColorGamutPoint p = new(X, Y);

        if (gamut.ContainsPoint(p))
            return this;

        ColorGamutPoint closest = gamut.GetClosestPoint(p);
        return new NDPColor(x: closest.X, y: closest.Y, brightness: Brightness);
    }

    public static NDPColor LerpUnclamped(NDPColor c1, NDPColor c2, double t)
    {
        return new(
            x: DoubleHelpers.LerpUnclamped(c1.X, c2.X, t),
            y: DoubleHelpers.LerpUnclamped(c1.Y, c2.Y, t),
            brightness: DoubleHelpers.LerpUnclamped(c1.Brightness, c2.Brightness, t)
        );
    }

    public static NDPColor Lerp(NDPColor c1, NDPColor c2, double t)
        => LerpUnclamped(c1, c2, t.Clamp01());

    // In the second answer after Mark's answer, there is a comparison with xyY color mixing
    // and that looked good, so I'll be using that instead.
    // public static NDPColor Gradient(NDPColor c1, NDPColor c2, double t)
    // {
    //     // Stolen from: https://stackoverflow.com/questions/22607043/color-gradient-algorithm/49321304#49321304

    //     (double r1, double g1, double b1) = c1.ToLinearRGB();
    //     (double r2, double g2, double b2) = c2.ToLinearRGB();

    //     double r = DoubleHelpers.Lerp(r1, r2, t);
    //     double g = DoubleHelpers.Lerp(g1, g2, t);
    //     double b = DoubleHelpers.Lerp(b1, b2, t);

    //     const double gamma = 0.43;
    //     double brightness1 = Math.Pow(r1 + g1 + b1, gamma);
    //     double brightness2 = Math.Pow(r2 + g2 + b2, gamma);

    //     double brightness = DoubleHelpers.Lerp(brightness1, brightness2, t);
    //     double intensity = Math.Pow(brightness, 1 / gamma);

    //     double rgbSum = r + g + b;
    //     if (rgbSum != 0)
    //     {
    //         double factor = intensity / rgbSum;
    //         r *= factor;
    //         g *= factor;
    //         b *= factor;
    //     }

    //     return FromLinearRGB(r, g, b);
    // }
}
