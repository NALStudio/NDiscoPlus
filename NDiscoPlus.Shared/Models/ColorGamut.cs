using NDiscoPlus.Shared.Models.Color;

namespace NDiscoPlus.Shared.Models;

public class ColorGamut
{
    public ColorGamutPoint Red { get; }
    public ColorGamutPoint Green { get; }
    public ColorGamutPoint Blue { get; }

    public ColorGamut(ColorGamutPoint red, ColorGamutPoint green, ColorGamutPoint blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public static readonly ColorGamut sRGB = new(
        new ColorGamutPoint(0.6400d, 0.3300d),
        new ColorGamutPoint(0.3000d, 0.6000d),
        new ColorGamutPoint(0.1500d, 0.0600d)
    );

    public static readonly ColorGamut DisplayP3 = new(
        new ColorGamutPoint(0.680d, 0.320d),
        new ColorGamutPoint(0.265d, 0.690d),
        new ColorGamutPoint(0.150d, 0.060d)
    );

    /// <summary>
    /// Philips Hue Gamut A. <br />
    /// Read more information <a href="https://developers.meethue.com/develop/application-design-guidance/color-conversion-formulas-rgb-to-xy-and-back/#Gamut">here</a>.
    /// </summary>
    public static readonly ColorGamut hueGamutA = new(
        new ColorGamutPoint(0.704d, 0.296d),
        new ColorGamutPoint(0.2151d, 0.7106d),
        new ColorGamutPoint(0.138d, 0.08d)
    );
    /// <summary>
    /// Philips Hue Gamut B. <br />
    /// Read more information <a href="https://developers.meethue.com/develop/application-design-guidance/color-conversion-formulas-rgb-to-xy-and-back/#Gamut">here</a>.
    /// </summary>
    public static readonly ColorGamut hueGamutB = new(
        new ColorGamutPoint(0.675d, 0.322d),
        new ColorGamutPoint(0.409d, 0.518d),
        new ColorGamutPoint(0.167d, 0.04d)
    );
    /// <summary>
    /// Philips Hue Gamut C. <br />
    /// Read more information <a href="https://developers.meethue.com/develop/application-design-guidance/color-conversion-formulas-rgb-to-xy-and-back/#Gamut">here</a>.
    /// </summary>
    public static readonly ColorGamut hueGamutC = new(
        new ColorGamutPoint(0.6915d, 0.3038d),
        new ColorGamutPoint(0.17d, 0.7d),
        new ColorGamutPoint(0.1532d, 0.0475d)
    );

    public bool ContainsPoint(ColorGamutPoint point)
    {
        // https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle/2049593#2049593

        static double Sign(ColorGamutPoint p1, ColorGamutPoint p2, ColorGamutPoint p3)
            => ((p1.X - p3.X) * (p2.Y - p3.Y)) - ((p2.X - p3.X) * (p1.Y - p3.Y));

        double d1 = Sign(point, Red, Green);
        double d2 = Sign(point, Green, Blue);
        double d3 = Sign(point, Blue, Red);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    // Reference from: https://developers.meethue.com/develop/application-design-guidance/color-conversion-formulas-rgb-to-xy-and-back
    // iOS implementation.
    public ColorGamutPoint GetClosestPoint(ColorGamutPoint point)
    {
        static double DistanceSquared(ColorGamutPoint p1, ColorGamutPoint p2)
        {
            double diffX = (p2.X - p1.X);
            double diffY = (p2.Y - p1.Y);
            return (diffX * diffX) + (diffY * diffY);
        }

        ColorGamutPoint p1 = GetClosestPointOnLineSegment(Red, Green, point);
        ColorGamutPoint p2 = GetClosestPointOnLineSegment(Green, Blue, point);
        ColorGamutPoint p3 = GetClosestPointOnLineSegment(Blue, Red, point);

        ColorGamutPoint[] points = [p1, p2, p3];
        return points.MinBy(p => DistanceSquared(point, p));
    }

    // https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line/9557244#9557244
    private static ColorGamutPoint GetClosestPointOnLineSegment(ColorGamutPoint A, ColorGamutPoint B, ColorGamutPoint P)
    {
        double APx = P.X - A.X;
        double APy = P.Y - A.Y;

        double ABx = B.X - A.X;
        double ABy = B.Y - A.Y;

        double magnitudeAB = (ABx * ABx) + (ABy * ABy); // vector AB length squared.
        double ABAPproduct = (APx * ABx) + (APy * ABy);
        double distance = ABAPproduct / magnitudeAB;

        if (distance < 0d)
            return A;
        if (distance > 1d)
            return B;

        double x = A.X + (ABx * distance);
        double y = A.Y + (ABy * distance);

        return new ColorGamutPoint(x, y);
    }
}

public readonly record struct ColorGamutPoint(double X, double Y)
{
    public NDPColor ToColor(double brightness = 1d)
        => new(x: X, y: Y, brightness: brightness);
}

public static class ColorGamutHelpers
{
    public static NDPColor GamutBlack(this ColorGamut? gamut)
    {
        if (gamut is null)
            return new();

        return gamut.Red.ToColor(brightness: 0d);
    }
}