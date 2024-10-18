using System.Runtime.CompilerServices;

namespace NDiscoPlus.Shared.Models.Color;

public readonly partial struct NDPColor : IEquatable<NDPColor>
{
    public double X { get; }
    public double Y { get; }
    public double Brightness { get; }

    public NDPColor(double x, double y, double brightness)
    {
        // I had a crash because we tried to send NaN to Philips Hue
        // Therefore I added these checks so that if it ever happens again,
        // I can catch it and actually diagnose the issue
        // since it was impossible to diagnose from the Philips Hue request
        ThrowIfNotFinite(x);
        ThrowIfNotFinite(y);
        ThrowIfNotFinite(brightness);

        // I could also check if values are in range, but I think that's a bit unnecessary
        // as the X and Y can in theory be any value since the CIE1931 color space doesn't limit its values into any specific range
        // Brightness on the other hand, is something we control and should be between 0 and 1.
        // I decided to not check this as well just to be consistent with XY checking.

        X = x;
        Y = y;
        Brightness = brightness;
    }

    public NDPColor CopyWith(double? x = null, double? y = null, double? brightness = null)
        => new(x ?? X, y ?? Y, brightness ?? Brightness);

    public override bool Equals(object? obj)
    {
        return obj is NDPColor color && Equals(color);
    }

    public bool Equals(NDPColor other)
    {
        return X == other.X &&
               Y == other.Y &&
               Brightness == other.Brightness;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Brightness);
    }

    public static bool operator ==(NDPColor left, NDPColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NDPColor left, NDPColor right)
    {
        return !(left == right);
    }

    private static void ThrowIfNotFinite(double value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(paramName, $"Value must be finite, not: '{value}'");
    }
}