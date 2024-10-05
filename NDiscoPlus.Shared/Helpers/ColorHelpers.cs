using NDiscoPlus.Shared.Models.Color;
using System.Globalization;

namespace NDiscoPlus.Shared.Helpers;
public static class ColorHelpers
{
    /// <summary>
    /// Arguments are clamped to the range 0-1.
    /// </summary>
    public static string ToHTMLColorRGB(double r, double g, double b, double alpha = 1d)
    {
        byte red = BitResolution.AsUInt8(r);
        byte green = BitResolution.AsUInt8(g);
        byte blue = BitResolution.AsUInt8(b);
        byte alpha_ = BitResolution.AsUInt8(alpha);

        return $"#{red:x2}{green:x2}{blue:x2}{alpha_:x2}";
    }

    public static string ToHTMLColorXYZ(double x, double y, double z)
    {
        return string.Format(CultureInfo.InvariantCulture, "color(xyz {0} {1} {2})", x, y, z);
    }
    public static string ToHTMLColorXYZ(NDPColor color)
    {
        (double x, double y, double z) = color.ToXYZ();
        return ToHTMLColorXYZ(x, y, z);
    }

    // public static RGBColor Lerp(RGBColor a, RGBColor b, double t)
    // {
    //     t = Math.Clamp(t, 0d, 1d);
    // 
    //     return new RGBColor(
    //         a.R + ((b.R - a.R) * t),
    //         a.G + ((b.G - a.G) * t),
    //         a.B + ((b.B - a.B) * t)
    //     );
    // }

    // https://en.wikipedia.org/wiki/SRGB#From_sRGB_to_CIE_XYZ
    public static double SRGBInverseCompanding(double c)
    {
        if (c <= 0.04045d)
            return c / 12.92d;
        else
            return Math.Pow((c + 0.055d) / 1.055d, 2.4d);
    }

    // https://en.wikipedia.org/wiki/SRGB#From_CIE_XYZ_to_sRGB
    public static double SRGBCompanding(double c)
    {
        if (c <= 0.0031308d)
            return c * 12.92d;
        else
            return (1.055d * Math.Pow(c, 1d / 2.4d)) - 0.055d;
    }
}
