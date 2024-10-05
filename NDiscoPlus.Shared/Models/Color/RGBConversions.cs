using NDiscoPlus.Shared.Helpers;

namespace NDiscoPlus.Shared.Models.Color;
public readonly partial struct NDPColor
{
    #region Linear RGB
    public static NDPColor FromLinearRGB(double r, double g, double b)
    {
        // Not needed as we don't use a gamma function
        // but we check just in case the user assumes that the values range from 0-255 and not 0-1
        // so the user gets an error instead of being puzzled like "wtf? why isn't this working?"
        if (r < 0d || r > 1d)
            throw new ArgumentOutOfRangeException(nameof(r));
        if (g < 0d || g > 1d)
            throw new ArgumentOutOfRangeException(nameof(g));
        if (b < 0d || b > 1d)
            throw new ArgumentOutOfRangeException(nameof(b));

        // No need to use more than four decimal places
        // as when the seven decimal place matrix is inversed
        // its values are equal to the four decimal ones when rounded to seven decimals
        double x = (0.4124d * r) + (0.3576d * g) + (0.1805d * b);
        double y = (0.2126d * r) + (0.7152d * g) + (0.0722d * b);
        double z = (0.0193d * r) + (0.1192d * g) + (0.9505d * b);

        return FromXYZ(x, y, z);
    }

    // https://en.wikipedia.org/wiki/SRGB#From_CIE_XYZ_to_sRGB
    // https://www.color.org/chardata/rgb/sRGB.pdf
    public (double R, double G, double B) ToLinearRGB()
    {
        (double x, double y, double z) = ToXYZ();

        double r = (3.2406255d * x) + (-1.537208d * y) + (-0.4986286d * z);
        double g = (-0.9689307d * x) + (1.8757561d * y) + (0.0415175d * z);
        double b = (0.0557101d * x) + (-0.2040211d * y) + (1.0569959d * z);

        return (r, g, b);
    }
    #endregion

    #region sRGB
    public static NDPColor FromSRGB(double r, double g, double b)
    {
        if (r < 0d || r > 1d)
            throw new ArgumentOutOfRangeException(nameof(r));
        if (g < 0d || g > 1d)
            throw new ArgumentOutOfRangeException(nameof(g));
        if (b < 0d || b > 1d)
            throw new ArgumentOutOfRangeException(nameof(b));

        r = ColorHelpers.SRGBInverseCompanding(r);
        g = ColorHelpers.SRGBInverseCompanding(g);
        b = ColorHelpers.SRGBInverseCompanding(b);

        return FromLinearRGB(r, g, b);
    }

    public (double R, double G, double B) ToSRGB()
    {
        (double r, double g, double b) = ToLinearRGB();

        r = ColorHelpers.SRGBCompanding(r);
        g = ColorHelpers.SRGBCompanding(g);
        b = ColorHelpers.SRGBCompanding(b);

        return (r, g, b);
    }
    #endregion

    #region Display P3
    public static NDPColor FromDisplayP3(double r, double g, double b)
    {
        if (r < 0d || r > 1d)
            throw new ArgumentOutOfRangeException(nameof(r));
        if (g < 0d || g > 1d)
            throw new ArgumentOutOfRangeException(nameof(g));
        if (b < 0d || b > 1d)
            throw new ArgumentOutOfRangeException(nameof(b));

        // Display P3 uses sRGB color component transfer function:
        // https://www.color.org/chardata/rgb/DisplayP3.xalter
        r = ColorHelpers.SRGBInverseCompanding(r);
        g = ColorHelpers.SRGBInverseCompanding(g);
        b = ColorHelpers.SRGBInverseCompanding(b);

        double x = (0.4866d * r) + (0.2657d * g) + (0.1982d * b);
        double y = (0.2290d * r) + (0.6917d * g) + (0.0793d * b);
        double z = (0.0000d * r) + (0.0451d * g) + (1.0439d * b);

        return FromXYZ(x, y, z);
    }

    public (double R, double G, double B) ToDisplayP3()
    {
        (double x, double y, double z) = ToXYZ();

        double r = (2.4934778d * x) + (-0.9315558d * y) + (-0.4026582d * z);
        double g = (-0.8296208d * x) + (1.7628536d * y) + (0.0236005d * z);
        double b = (0.0358424d * x) + (-0.0761612d * y) + (0.9569265d * z);

        r = ColorHelpers.SRGBCompanding(r);
        g = ColorHelpers.SRGBCompanding(g);
        b = ColorHelpers.SRGBCompanding(b);

        return (r, g, b);
    }
    #endregion
}
